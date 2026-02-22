#!/usr/bin/env python3
"""Tests for extract_spec_options.py"""

import json
import os
import shutil
import sys
import tempfile
import unittest

from extract_spec_options import (
    parse_spec_option,
    parse_spec_capabilities,
    process_modules,
    process_capabilities,
    generate_js,
    generate_capabilities_js,
    generate_coverage_report,
    load_modules_from_dir,
    to_snake_id,
)


class TestParseSpecOption(unittest.TestCase):
    """Tests for parsing [SpecOption] attributes from C# code."""

    def test_single_line_attribute(self):
        code = '''
        [SpecOption(Category = "Revaluation", Name = "Fixed Rate", Description = "Fixed annual rate.")]
        public class FixedRateReval : IRevalStrategy { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['Category'], 'Revaluation')
        self.assertEqual(result['Name'], 'Fixed Rate')
        self.assertEqual(result['Description'], 'Fixed annual rate.')
        self.assertEqual(result['CodeClass'], 'FixedRateReval')

    def test_multiline_attribute(self):
        code = '''
        [SpecOption(
            Category = "Commutation",
            Name = "Trivial Commutation",
            Description = "Full commutation of small pots.",
            WhyItMatters = "Allows small lump sum payouts."
        )]
        public class TrivialCommutation : ICommutationStrategy { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['Category'], 'Commutation')
        self.assertEqual(result['Name'], 'Trivial Commutation')
        self.assertEqual(result['WhyItMatters'], 'Allows small lump sum payouts.')
        self.assertEqual(result['CodeClass'], 'TrivialCommutation')

    def test_missing_optional_why_it_matters(self):
        code = '''
        [SpecOption(Category = "GMP", Name = "Basic GMP", Description = "Basic GMP calc.")]
        public class BasicGmp : IGmpStrategy { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertNotIn('WhyItMatters', result)
        self.assertEqual(result['Name'], 'Basic GMP')

    def test_no_attribute(self):
        code = '''
        public class InternalHelper
        {
            public static string Format(DateTime d) => d.ToString("yyyy-MM-dd");
        }
        '''
        result = parse_spec_option(code)
        self.assertIsNone(result)

    def test_missing_required_category(self):
        code = '''
        [SpecOption(Name = "Orphan")]
        public class Orphan { }
        '''
        result = parse_spec_option(code)
        self.assertIsNone(result)

    def test_missing_required_name(self):
        code = '''
        [SpecOption(Category = "GMP")]
        public class Nameless { }
        '''
        result = parse_spec_option(code)
        self.assertIsNone(result)

    def test_empty_code(self):
        result = parse_spec_option('')
        self.assertIsNone(result)


class TestToSnakeId(unittest.TestCase):

    def test_pascal_case(self):
        self.assertEqual(to_snake_id('CpiCappedRevaluation'), 'cpi_capped_revaluation')

    def test_simple_word(self):
        self.assertEqual(to_snake_id('Trivial'), 'trivial')

    def test_with_spaces(self):
        self.assertEqual(to_snake_id('Some Name'), 'some_name')

    def test_consecutive_caps(self):
        self.assertEqual(to_snake_id('GMPEqualiser'), 'gmp_equaliser')


class TestProcessModules(unittest.TestCase):

    def test_groups_by_category(self):
        modules = [
            {
                'moduleName': 'A',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "Revaluation", Name = "A", Description = "Desc A")]\npublic class A { }',
                'lastModified': '2025-01-01T00:00:00'
            },
            {
                'moduleName': 'B',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "Commutation", Name = "B", Description = "Desc B")]\npublic class B { }',
                'lastModified': '2025-02-01T00:00:00'
            },
            {
                'moduleName': 'C',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "Revaluation", Name = "C", Description = "Desc C")]\npublic class C { }',
                'lastModified': '2025-03-01T00:00:00'
            },
        ]
        result = process_modules(modules)
        self.assertIn('revaluation', result)
        self.assertIn('commutation', result)
        self.assertEqual(len(result['revaluation']), 2)
        self.assertEqual(len(result['commutation']), 1)

    def test_skips_modules_without_attribute(self):
        modules = [
            {
                'moduleName': 'Helper',
                'scheme': 'Core',
                'code': 'public class Helper { }',
                'lastModified': '2025-01-01T00:00:00'
            },
        ]
        result = process_modules(modules)
        self.assertEqual(len(result), 0)

    def test_skips_empty_code(self):
        modules = [{'moduleName': 'Empty', 'scheme': 'Core', 'code': '', 'lastModified': ''}]
        result = process_modules(modules)
        self.assertEqual(len(result), 0)

    def test_date_truncated(self):
        modules = [
            {
                'moduleName': 'X',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "GMP", Name = "X", Description = "X")]\npublic class X { }',
                'lastModified': '2025-11-15T10:30:00'
            },
        ]
        result = process_modules(modules)
        self.assertEqual(result['gmp'][0]['lastModified'], '2025-11-15')

    def test_builder_category_pluralised(self):
        modules = [
            {
                'moduleName': 'MyBuilder',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "Builder", Name = "My Builder", Description = "Builds stuff")]\npublic class MyBuilder { }',
                'lastModified': '2025-01-01T00:00:00'
            },
        ]
        result = process_modules(modules)
        self.assertIn('builders', result)
        self.assertNotIn('builder', result)

    def test_why_it_matters_included_when_present(self):
        modules = [
            {
                'moduleName': 'W',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "GMP", Name = "W", Description = "D", WhyItMatters = "Important")]\npublic class W { }',
                'lastModified': ''
            },
        ]
        result = process_modules(modules)
        self.assertEqual(result['gmp'][0]['whyItMatters'], 'Important')

    def test_why_it_matters_omitted_when_absent(self):
        modules = [
            {
                'moduleName': 'V',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "GMP", Name = "V", Description = "D")]\npublic class V { }',
                'lastModified': ''
            },
        ]
        result = process_modules(modules)
        self.assertNotIn('whyItMatters', result['gmp'][0])


class TestGenerateJs(unittest.TestCase):

    def test_generates_valid_structure(self):
        categories = {
            'revaluation': [
                {
                    'id': 'fixed_rate',
                    'name': 'Fixed Rate',
                    'description': 'Fixed annual rate.',
                    'codeClass': 'FixedRate',
                    'scheme': 'Core',
                    'lastModified': '2025-01-01',
                }
            ]
        }
        js = generate_js(categories)
        self.assertIn('const CODE_OPTIONS = {', js)
        self.assertIn('revaluation: [', js)
        self.assertIn('id: "fixed_rate"', js)
        self.assertIn('name: "Fixed Rate"', js)
        self.assertIn('};', js)

    def test_includes_why_it_matters_when_present(self):
        categories = {
            'gmp': [
                {
                    'id': 'gmp_eq',
                    'name': 'GMP Eq',
                    'description': 'Equalisation.',
                    'whyItMatters': 'Required for fairness.',
                    'codeClass': 'GmpEq',
                    'scheme': 'Core',
                    'lastModified': '2025-01-01',
                }
            ]
        }
        js = generate_js(categories)
        self.assertIn('whyItMatters: "Required for fairness."', js)

    def test_omits_why_it_matters_when_absent(self):
        categories = {
            'gmp': [
                {
                    'id': 'basic',
                    'name': 'Basic',
                    'description': 'Basic.',
                    'codeClass': 'Basic',
                    'scheme': 'Core',
                    'lastModified': '2025-01-01',
                }
            ]
        }
        js = generate_js(categories)
        self.assertNotIn('whyItMatters', js)

    def test_empty_categories(self):
        js = generate_js({})
        self.assertEqual(js, 'const CODE_OPTIONS = {\n};\n')

    def test_multiple_categories_sorted(self):
        categories = {
            'commutation': [{'id': 'a', 'name': 'A', 'description': '', 'codeClass': 'A', 'scheme': '', 'lastModified': ''}],
            'revaluation': [{'id': 'b', 'name': 'B', 'description': '', 'codeClass': 'B', 'scheme': '', 'lastModified': ''}],
        }
        js = generate_js(categories)
        comm_pos = js.index('commutation:')
        reval_pos = js.index('revaluation:')
        self.assertLess(comm_pos, reval_pos)


class TestParseSpecCapabilities(unittest.TestCase):
    """Tests for parsing [SpecCapability] attributes from C# code."""

    def test_single_line_attribute(self):
        code = '''
        public class Foo {
            [SpecCapability(Category = "Transfers", Name = "Club Transfer", Description = "Calculates club transfer.")]
            public decimal CalculateClub(decimal pension, decimal factor)
            {
                return pension * factor;
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['Category'], 'Transfers')
        self.assertEqual(result[0]['Name'], 'Club Transfer')
        self.assertEqual(result[0]['methodName'], 'CalculateClub')
        self.assertEqual(result[0]['returnType'], 'decimal')
        self.assertEqual(result[0]['parameters'], 'decimal pension, decimal factor')

    def test_multiline_attribute(self):
        code = '''
        public class Foo {
            [SpecCapability(
                Category = "GMP",
                Name = "Anti-Franking",
                Description = "Checks anti-franking.",
                WhyItMatters = "Prevents reduction."
            )]
            public bool CheckAntiFranking(decimal total, decimal gmp)
            {
                return true;
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['Category'], 'GMP')
        self.assertEqual(result[0]['WhyItMatters'], 'Prevents reduction.')
        self.assertEqual(result[0]['methodName'], 'CheckAntiFranking')
        self.assertEqual(result[0]['returnType'], 'bool')

    def test_multiple_methods_in_one_module(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "A", Name = "First", Description = "First method.")]
            public int DoFirst(int x)
            {
                return x;
            }

            [SpecCapability(Category = "B", Name = "Second", Description = "Second method.")]
            public string DoSecond(string s, int n)
            {
                return s;
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 2)
        self.assertEqual(result[0]['Name'], 'First')
        self.assertEqual(result[0]['methodName'], 'DoFirst')
        self.assertEqual(result[1]['Name'], 'Second')
        self.assertEqual(result[1]['methodName'], 'DoSecond')

    def test_missing_optional_why_it_matters(self):
        code = '''
        public class Foo {
            [SpecCapability(Category = "X", Name = "Y", Description = "Z")]
            public void DoThing()
            {
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertNotIn('WhyItMatters', result[0])

    def test_no_attribute(self):
        code = '''
        public class Foo {
            public void DoThing() { }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 0)

    def test_auto_extracted_signature(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "Tax", Name = "LTA Check", Description = "Checks LTA.")]
            public bool CheckLifetimeAllowance(decimal totalBenefits, decimal ltaLimit)
            {
                return totalBenefits <= ltaLimit;
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(result[0]['returnType'], 'bool')
        self.assertEqual(result[0]['methodName'], 'CheckLifetimeAllowance')
        self.assertEqual(result[0]['parameters'], 'decimal totalBenefits, decimal ltaLimit')

    def test_missing_required_fields(self):
        code = '''
        public class Foo {
            [SpecCapability(Name = "NoCategory")]
            public void Do() { }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 0)


class TestProcessCapabilities(unittest.TestCase):

    def test_groups_by_category(self):
        modules = [
            {
                'moduleName': 'Calc',
                'scheme': 'Core',
                'code': 'public class Calc {\n[SpecCapability(Category = "GMP", Name = "A", Description = "A")]\npublic int DoA(int x)\n{\nreturn x;\n}\n[SpecCapability(Category = "Transfers", Name = "B", Description = "B")]\npublic int DoB(int y)\n{\nreturn y;\n}\n}',
                'lastModified': '2025-01-01T00:00:00'
            },
        ]
        result = process_capabilities(modules)
        self.assertIn('gmp', result)
        self.assertIn('transfers', result)
        self.assertEqual(len(result['gmp']), 1)
        self.assertEqual(len(result['transfers']), 1)

    def test_skips_modules_without_capability(self):
        modules = [
            {
                'moduleName': 'Helper',
                'scheme': 'Core',
                'code': 'public class Helper { public void Do() { } }',
                'lastModified': '2025-01-01T00:00:00'
            },
        ]
        result = process_capabilities(modules)
        self.assertEqual(len(result), 0)

    def test_date_truncated(self):
        modules = [
            {
                'moduleName': 'X',
                'scheme': 'Core',
                'code': 'public class X {\n[SpecCapability(Category = "A", Name = "Y", Description = "Z")]\npublic void Do()\n{\n}\n}',
                'lastModified': '2025-11-15T10:30:00'
            },
        ]
        result = process_capabilities(modules)
        self.assertEqual(result['a'][0]['lastModified'], '2025-11-15')

    def test_includes_method_signature_fields(self):
        modules = [
            {
                'moduleName': 'Calc',
                'scheme': 'Core',
                'code': 'public class Calc {\n[SpecCapability(Category = "Tax", Name = "Check", Description = "Check LTA")]\npublic bool CheckLta(decimal total, decimal limit)\n{\nreturn true;\n}\n}',
                'lastModified': ''
            },
        ]
        result = process_capabilities(modules)
        cap = result['tax'][0]
        self.assertEqual(cap['methodName'], 'CheckLta')
        self.assertEqual(cap['returnType'], 'bool')
        self.assertEqual(cap['parameters'], 'decimal total, decimal limit')
        self.assertEqual(cap['codeClass'], 'Calc')

    def test_parent_option_included_when_present(self):
        modules = [
            {
                'moduleName': 'GmpCalc',
                'scheme': 'Core',
                'code': '[SpecOption(Category = "GMP", Name = "GMP Equalisation", Description = "Equalise GMP")]\npublic class GmpCalc {\n[SpecCapability(Category = "GMP", Name = "Anti-Frank", Description = "Check franking")]\npublic bool Check(decimal x)\n{\nreturn true;\n}\n}',
                'lastModified': ''
            },
        ]
        result = process_capabilities(modules)
        cap = result['gmp'][0]
        self.assertIn('parentOption', cap)
        self.assertEqual(cap['parentOption']['id'], 'gmp_calc')
        self.assertEqual(cap['parentOption']['name'], 'GMP Equalisation')

    def test_parent_option_absent_when_no_spec_option(self):
        modules = [
            {
                'moduleName': 'Standalone',
                'scheme': 'Core',
                'code': 'public class Standalone {\n[SpecCapability(Category = "Tax", Name = "LTA", Description = "Check LTA")]\npublic bool Check()\n{\nreturn true;\n}\n}',
                'lastModified': ''
            },
        ]
        result = process_capabilities(modules)
        cap = result['tax'][0]
        self.assertNotIn('parentOption', cap)


class TestGenerateCapabilitiesJs(unittest.TestCase):

    def test_generates_valid_structure(self):
        categories = {
            'transfers': [
                {
                    'id': 'calculate_partial_cetv',
                    'name': 'Partial CETV',
                    'description': 'Calculates partial CETV.',
                    'methodName': 'CalculatePartialCetv',
                    'returnType': 'decimal',
                    'parameters': 'decimal totalCetv, decimal proportion',
                    'codeClass': 'TransferCalculator',
                    'scheme': 'Core',
                    'lastModified': '2025-11-15',
                }
            ]
        }
        js = generate_capabilities_js(categories)
        self.assertIn('const CODE_CAPABILITIES = {', js)
        self.assertIn('transfers: [', js)
        self.assertIn('id: "calculate_partial_cetv"', js)
        self.assertIn('methodName: "CalculatePartialCetv"', js)
        self.assertIn('returnType: "decimal"', js)
        self.assertIn('parameters: "decimal totalCetv, decimal proportion"', js)
        self.assertIn('};', js)

    def test_sorted_categories(self):
        categories = {
            'transfers': [{'id': 'a', 'name': 'A', 'description': '', 'codeClass': 'A', 'scheme': '', 'lastModified': ''}],
            'gmp': [{'id': 'b', 'name': 'B', 'description': '', 'codeClass': 'B', 'scheme': '', 'lastModified': ''}],
        }
        js = generate_capabilities_js(categories)
        gmp_pos = js.index('gmp:')
        transfers_pos = js.index('transfers:')
        self.assertLess(gmp_pos, transfers_pos)

    def test_includes_parent_option_when_present(self):
        categories = {
            'gmp': [
                {
                    'id': 'check_anti_franking',
                    'name': 'Anti-Franking Check',
                    'description': 'Checks franking.',
                    'methodName': 'CheckAntiFranking',
                    'returnType': 'bool',
                    'parameters': 'decimal x',
                    'parentOption': {'id': 'gmp_equaliser', 'name': 'GMP Equalisation'},
                    'codeClass': 'GmpEqualiser',
                    'scheme': 'Core',
                    'lastModified': '2025-01-01',
                }
            ]
        }
        js = generate_capabilities_js(categories)
        self.assertIn('parentOption: { id: "gmp_equaliser", name: "GMP Equalisation" }', js)

    def test_omits_parent_option_when_absent(self):
        categories = {
            'transfers': [
                {
                    'id': 'calc_cetv',
                    'name': 'CETV',
                    'description': 'Calc.',
                    'codeClass': 'Calc',
                    'scheme': 'Core',
                    'lastModified': '',
                }
            ]
        }
        js = generate_capabilities_js(categories)
        self.assertNotIn('parentOption', js)

    def test_empty_input(self):
        js = generate_capabilities_js({})
        self.assertEqual(js, 'const CODE_CAPABILITIES = {\n};\n')


class TestLoadModulesFromDir(unittest.TestCase):
    """Tests for loading modules from a directory of .cs files."""

    def setUp(self):
        self.tmpdir = tempfile.mkdtemp()

    def tearDown(self):
        shutil.rmtree(self.tmpdir)

    def _write_file(self, rel_path, content):
        full_path = os.path.join(self.tmpdir, rel_path)
        os.makedirs(os.path.dirname(full_path), exist_ok=True)
        with open(full_path, 'w') as f:
            f.write(content)
        return full_path

    def test_correct_module_count(self):
        self._write_file('Core/A.cs', 'public class A { }')
        self._write_file('Core/B.cs', 'public class B { }')
        self._write_file('Core/readme.txt', 'not a cs file')
        modules = load_modules_from_dir(self.tmpdir)
        self.assertEqual(len(modules), 2)

    def test_scheme_from_subdirectory(self):
        self._write_file('Core/A.cs', 'public class A { }')
        self._write_file('SchemeXYZ/B.cs', 'public class B { }')
        modules = load_modules_from_dir(self.tmpdir)
        schemes = {m['moduleName']: m['scheme'] for m in modules}
        self.assertEqual(schemes['A'], 'Core')
        self.assertEqual(schemes['B'], 'SchemeXYZ')

    def test_last_modified_populated(self):
        self._write_file('Core/A.cs', 'public class A { }')
        modules = load_modules_from_dir(self.tmpdir)
        self.assertIn('T', modules[0]['lastModified'])  # ISO format with T separator

    def test_module_name_from_filename(self):
        self._write_file('Core/MyCalculator.cs', 'public class MyCalculator { }')
        modules = load_modules_from_dir(self.tmpdir)
        self.assertEqual(modules[0]['moduleName'], 'MyCalculator')

    def test_code_content_read(self):
        self._write_file('Core/A.cs', 'public class A { /* body */ }')
        modules = load_modules_from_dir(self.tmpdir)
        self.assertIn('/* body */', modules[0]['code'])

    def test_empty_directory(self):
        modules = load_modules_from_dir(self.tmpdir)
        self.assertEqual(len(modules), 0)

    def test_file_at_root_has_empty_scheme(self):
        self._write_file('RootFile.cs', 'public class RootFile { }')
        modules = load_modules_from_dir(self.tmpdir)
        self.assertEqual(modules[0]['scheme'], '')

    def test_sample_modules_directory(self):
        """Integration test using the actual sample-modules directory."""
        sample_dir = os.path.join(os.path.dirname(__file__), 'sample-modules')
        if not os.path.isdir(sample_dir):
            self.skipTest('sample-modules directory not found')
        modules = load_modules_from_dir(sample_dir)
        self.assertEqual(len(modules), 6)
        # All should have scheme 'Core'
        for m in modules:
            self.assertEqual(m['scheme'], 'Core')


class TestCoverageReport(unittest.TestCase):
    """Tests for the coverage report generation."""

    def test_counts_per_class(self):
        modules = [
            {
                'moduleName': 'GmpEqualiser',
                'scheme': 'Core',
                'code': (
                    '[SpecOption(Category = "GMP", Name = "GMP Eq", Description = "Eq")]\n'
                    'public class GmpEqualiser {\n'
                    '    [SpecCapability(Category = "GMP", Name = "A", Description = "A")]\n'
                    '    public bool CheckA(decimal x) {\n'
                    '        return true;\n'
                    '    }\n'
                    '    public decimal Equalise(decimal a, decimal b) {\n'
                    '        return a;\n'
                    '    }\n'
                    '}\n'
                ),
                'lastModified': ''
            },
        ]
        report = generate_coverage_report(modules)
        self.assertIn('GmpEqualiser: 1/2 methods documented (50%)', report)

    def test_fully_documented_class(self):
        modules = [
            {
                'moduleName': 'Calc',
                'scheme': 'Core',
                'code': (
                    '[SpecOption(Category = "Tax", Name = "Tax", Description = "Tax")]\n'
                    'public class Calc {\n'
                    '    [SpecCapability(Category = "Tax", Name = "Check", Description = "Check")]\n'
                    '    public bool Check() {\n'
                    '        return true;\n'
                    '    }\n'
                    '}\n'
                ),
                'lastModified': ''
            },
        ]
        report = generate_coverage_report(modules)
        self.assertIn('Calc: 1/1 methods documented (100%)', report)

    def test_no_public_methods(self):
        modules = [
            {
                'moduleName': 'Empty',
                'scheme': 'Core',
                'code': (
                    '[SpecOption(Category = "GMP", Name = "Empty", Description = "Empty")]\n'
                    'public class Empty {\n'
                    '    private void Internal() { }\n'
                    '}\n'
                ),
                'lastModified': ''
            },
        ]
        report = generate_coverage_report(modules)
        self.assertIn('Empty: 0/0 methods documented (n/a)', report)

    def test_overall_totals(self):
        modules = [
            {
                'moduleName': 'A',
                'scheme': 'Core',
                'code': (
                    '[SpecOption(Category = "GMP", Name = "A", Description = "A")]\n'
                    'public class A {\n'
                    '    [SpecCapability(Category = "GMP", Name = "Do", Description = "Do")]\n'
                    '    public void Do() {\n'
                    '    }\n'
                    '    public void Other() {\n'
                    '    }\n'
                    '}\n'
                ),
                'lastModified': ''
            },
            {
                'moduleName': 'B',
                'scheme': 'Core',
                'code': (
                    '[SpecOption(Category = "GMP", Name = "B", Description = "B")]\n'
                    'public class B {\n'
                    '    [SpecCapability(Category = "GMP", Name = "Run", Description = "Run")]\n'
                    '    public void Run() {\n'
                    '    }\n'
                    '}\n'
                ),
                'lastModified': ''
            },
        ]
        report = generate_coverage_report(modules)
        self.assertIn('Overall: 2/3 methods documented (67%)', report)

    def test_skips_classes_without_spec_option(self):
        modules = [
            {
                'moduleName': 'Helper',
                'scheme': 'Core',
                'code': 'public class Helper {\n    public void Do() {\n    }\n}',
                'lastModified': ''
            },
        ]
        report = generate_coverage_report(modules)
        self.assertIn('No classes with [SpecOption] found', report)

    def test_excludes_constructors(self):
        """Constructors (method name == class name) should not count as public methods."""
        modules = [
            {
                'moduleName': 'Calc',
                'scheme': 'Core',
                'code': (
                    '[SpecOption(Category = "GMP", Name = "Calc", Description = "Calc")]\n'
                    'public class Calc {\n'
                    '    public Calc() {\n'
                    '    }\n'
                    '    public void Do() {\n'
                    '    }\n'
                    '}\n'
                ),
                'lastModified': ''
            },
        ]
        report = generate_coverage_report(modules)
        # Constructor should not be counted, so 0/1 not 0/2
        self.assertIn('Calc: 0/1 methods documented (0%)', report)


class TestRobustParsing(unittest.TestCase):
    """Tests for regex robustness against XML doc comments, #region, stacked attributes, generics."""

    def test_xml_doc_comments_between_attribute_and_class(self):
        code = '''
        [SpecOption(Category = "GMP", Name = "GMP Eq", Description = "Equalisation.")]
        /// <summary>
        /// Handles GMP equalisation using dual-record method.
        /// </summary>
        public class GmpEqualiser : IGmpStrategy { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['CodeClass'], 'GmpEqualiser')

    def test_region_between_attribute_and_class(self):
        code = '''
        [SpecOption(Category = "Revaluation", Name = "CPI", Description = "CPI reval.")]
        #region Implementation
        public class CpiReval : IRevalStrategy { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['CodeClass'], 'CpiReval')

    def test_stacked_attributes(self):
        code = '''
        [SpecOption(Category = "Transfers", Name = "Club", Description = "Club transfer.")]
        [Serializable]
        [DebuggerDisplay("Club")]
        public class ClubTransfer : ITransferStrategy { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['CodeClass'], 'ClubTransfer')

    def test_xml_comments_and_stacked_attributes_combined(self):
        code = '''
        [SpecOption(Category = "GMP", Name = "Section 148", Description = "S148 reval.")]
        /// <summary>Revalues GMP.</summary>
        [Obsolete("Use new version")]
        public class Section148Reval : IReval { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['CodeClass'], 'Section148Reval')

    def test_generic_return_type_on_capability(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "Async", Name = "Async Calc", Description = "Async calculation.")]
            public Task<decimal> CalculateAsync(decimal pension)
            {
                return Task.FromResult(pension);
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['methodName'], 'CalculateAsync')
        self.assertIn('Task<decimal>', result[0]['returnType'])

    def test_xml_doc_comments_between_capability_and_method(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "GMP", Name = "Check", Description = "Checks GMP.")]
            /// <summary>
            /// Performs the anti-franking check.
            /// </summary>
            /// <param name="total">Total pension amount.</param>
            public bool Check(decimal total)
            {
                return true;
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['methodName'], 'Check')
        self.assertEqual(result[0]['returnType'], 'bool')

    def test_region_between_capability_and_method(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "Tax", Name = "LTA", Description = "LTA check.")]
            #region LTA Implementation
            public bool CheckLta(decimal total)
            {
                return true;
            }
            #endregion
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['methodName'], 'CheckLta')

    def test_stacked_attributes_on_capability(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "GMP", Name = "Equalise", Description = "Equalises.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public decimal Equalise(decimal a, decimal b)
            {
                return Math.Max(a, b);
            }
        }
        '''
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['methodName'], 'Equalise')

    def test_expression_bodied_method(self):
        code = '''
        public class Calc {
            [SpecCapability(Category = "Tax", Name = "Rate", Description = "Gets rate.")]
            public decimal GetRate(decimal factor) => factor * 1.5m;
        }
        '''
        # Expression-bodied members use => not { so the PUBLIC_METHOD_RE won't match,
        # but the SpecCapability method sig regex should still capture the signature
        result = parse_spec_capabilities(code)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['methodName'], 'GetRate')
        self.assertEqual(result[0]['returnType'], 'decimal')

    def test_sealed_class_with_spec_option(self):
        code = '''
        [SpecOption(Category = "GMP", Name = "Sealed GMP", Description = "Sealed.")]
        public sealed class SealedGmp : IGmp { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['CodeClass'], 'SealedGmp')

    def test_abstract_class_with_spec_option(self):
        code = '''
        [SpecOption(Category = "GMP", Name = "Abstract GMP", Description = "Abstract.")]
        public abstract class AbstractGmp : IGmp { }
        '''
        result = parse_spec_option(code)
        self.assertIsNotNone(result)
        self.assertEqual(result['CodeClass'], 'AbstractGmp')


class TestEndToEnd(unittest.TestCase):
    """Test the full pipeline using sample-input.json."""

    def test_sample_input_options(self):
        sample_path = os.path.join(os.path.dirname(__file__), 'sample-input.json')
        with open(sample_path) as f:
            modules = json.load(f)

        categories = process_modules(modules)

        # Should have 3 categories from the sample data
        self.assertEqual(len(categories), 3)
        self.assertIn('revaluation', categories)
        self.assertIn('commutation', categories)
        self.assertIn('gmp', categories)

        # 2 revaluation options, 1 commutation, 1 GMP
        self.assertEqual(len(categories['revaluation']), 2)
        self.assertEqual(len(categories['commutation']), 1)
        self.assertEqual(len(categories['gmp']), 1)

        # InternalHelper and TransferCalculator (no SpecOption) should be skipped
        all_classes = [opt['codeClass'] for opts in categories.values() for opt in opts]
        self.assertNotIn('InternalHelper', all_classes)
        self.assertNotIn('TransferCalculator', all_classes)

        # JS output should be valid
        js = generate_js(categories)
        self.assertIn('const CODE_OPTIONS = {', js)

    def test_sample_input_capabilities(self):
        sample_path = os.path.join(os.path.dirname(__file__), 'sample-input.json')
        with open(sample_path) as f:
            modules = json.load(f)

        categories = process_capabilities(modules)

        # Should have 3 capability categories
        self.assertEqual(len(categories), 3)
        self.assertIn('revaluation', categories)
        self.assertIn('gmp', categories)
        self.assertIn('transfers', categories)

        # 1 revaluation capability, 2 GMP, 2 transfers
        self.assertEqual(len(categories['revaluation']), 1)
        self.assertEqual(len(categories['gmp']), 2)
        self.assertEqual(len(categories['transfers']), 2)

        # Check auto-extracted method signatures
        partial_cetv = [c for c in categories['transfers'] if c['name'] == 'Partial CETV'][0]
        self.assertEqual(partial_cetv['methodName'], 'CalculatePartialCetv')
        self.assertEqual(partial_cetv['returnType'], 'decimal')
        self.assertIn('decimal totalCetv', partial_cetv['parameters'])
        self.assertEqual(partial_cetv['codeClass'], 'TransferCalculator')

        # TransferCalculator has no SpecOption, so no parentOption
        self.assertNotIn('parentOption', partial_cetv)

        # GmpEqualiser capabilities should have parentOption linking to the SpecOption
        anti_franking = [c for c in categories['gmp'] if c['name'] == 'Anti-Franking Check'][0]
        self.assertIn('parentOption', anti_franking)
        self.assertEqual(anti_franking['parentOption']['id'], 'gmp_equaliser')
        self.assertEqual(anti_franking['parentOption']['name'], 'GMP Equalisation (Dual Record)')

        # JS output should be valid
        js = generate_capabilities_js(categories)
        self.assertIn('const CODE_CAPABILITIES = {', js)

    def test_options_and_capabilities_coexist(self):
        """Modules with both SpecOption and SpecCapability produce entries in both outputs."""
        sample_path = os.path.join(os.path.dirname(__file__), 'sample-input.json')
        with open(sample_path) as f:
            modules = json.load(f)

        options = process_modules(modules)
        capabilities = process_capabilities(modules)

        # GmpEqualiser has both a SpecOption and SpecCapability
        option_classes = [opt['codeClass'] for opts in options.values() for opt in opts]
        cap_classes = [cap['codeClass'] for caps in capabilities.values() for cap in caps]
        self.assertIn('GmpEqualiser', option_classes)
        self.assertIn('GmpEqualiser', cap_classes)


class TestEndToEndDirectory(unittest.TestCase):
    """Test the full pipeline using sample-modules/ directory."""

    def setUp(self):
        self.sample_dir = os.path.join(os.path.dirname(__file__), 'sample-modules')
        if not os.path.isdir(self.sample_dir):
            self.skipTest('sample-modules directory not found')

    def test_directory_mode_options(self):
        modules = load_modules_from_dir(self.sample_dir)
        categories = process_modules(modules)

        self.assertEqual(len(categories), 3)
        self.assertIn('revaluation', categories)
        self.assertIn('commutation', categories)
        self.assertIn('gmp', categories)
        self.assertEqual(len(categories['revaluation']), 2)
        self.assertEqual(len(categories['commutation']), 1)
        self.assertEqual(len(categories['gmp']), 1)

    def test_directory_mode_capabilities(self):
        modules = load_modules_from_dir(self.sample_dir)
        categories = process_capabilities(modules)

        self.assertEqual(len(categories), 3)
        self.assertIn('revaluation', categories)
        self.assertIn('gmp', categories)
        self.assertIn('transfers', categories)
        self.assertEqual(len(categories['revaluation']), 1)
        self.assertEqual(len(categories['gmp']), 2)
        self.assertEqual(len(categories['transfers']), 2)

    def test_directory_vs_json_identical_options(self):
        """Directory mode and JSON mode should produce identical JS output (ignoring lastModified)."""
        # Load from directory
        dir_modules = load_modules_from_dir(self.sample_dir)
        dir_categories = process_modules(dir_modules)

        # Load from JSON
        json_path = os.path.join(os.path.dirname(__file__), 'sample-input.json')
        with open(json_path) as f:
            json_modules = json.load(f)
        json_categories = process_modules(json_modules)

        # Same categories and same number of options
        self.assertEqual(sorted(dir_categories.keys()), sorted(json_categories.keys()))
        for cat in dir_categories:
            dir_names = sorted(opt['name'] for opt in dir_categories[cat])
            json_names = sorted(opt['name'] for opt in json_categories[cat])
            self.assertEqual(dir_names, json_names, f"Mismatch in category {cat}")

    def test_directory_vs_json_identical_capabilities(self):
        """Directory mode and JSON mode should produce identical capability names."""
        dir_modules = load_modules_from_dir(self.sample_dir)
        dir_categories = process_capabilities(dir_modules)

        json_path = os.path.join(os.path.dirname(__file__), 'sample-input.json')
        with open(json_path) as f:
            json_modules = json.load(f)
        json_categories = process_capabilities(json_modules)

        self.assertEqual(sorted(dir_categories.keys()), sorted(json_categories.keys()))
        for cat in dir_categories:
            dir_names = sorted(cap['name'] for cap in dir_categories[cat])
            json_names = sorted(cap['name'] for cap in json_categories[cat])
            self.assertEqual(dir_names, json_names, f"Mismatch in category {cat}")

    def test_coverage_report_from_directory(self):
        modules = load_modules_from_dir(self.sample_dir)
        report = generate_coverage_report(modules)
        self.assertIn('Coverage Report:', report)
        self.assertIn('GmpEqualiser:', report)
        self.assertIn('Overall:', report)


if __name__ == '__main__':
    unittest.main()
