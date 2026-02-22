#!/usr/bin/env python3
"""Parse C# [SpecOption] and [SpecCapability] attributes from .cs files or JSON and generate JS files."""

import argparse
import json
import os
import re
import sys
from datetime import datetime


# Matches [SpecOption(...)] including multiline, capturing the attribute body
SPEC_OPTION_RE = re.compile(
    r'\[SpecOption\s*\((.*?)\)\s*\]',
    re.DOTALL
)

# Matches a named parameter like Category = "Revaluation"
PARAM_RE = re.compile(
    r'(\w+)\s*=\s*"((?:[^"\\]|\\.)*)"'
)

# Matches the class name from a class declaration following the attribute.
# Robust: skips XML doc comments (/// ...), other attributes ([...]),
# #region/#endregion, and blank lines between attribute and class declaration.
CLASS_NAME_RE = re.compile(
    r'\[SpecOption\s*\(.*?\)\s*\]'
    r'(?:\s*(?:///[^\n]*|//[^\n]*|\#(?:region|endregion)[^\n]*|\[(?!SpecOption)[^\]]*\]))*'
    r'\s*public\s+(?:sealed\s+|abstract\s+|static\s+|partial\s+)*class\s+(\w+)',
    re.DOTALL
)

# Matches [SpecCapability(...)] including multiline, capturing the attribute body
SPEC_CAPABILITY_RE = re.compile(
    r'\[SpecCapability\s*\((.*?)\)\s*\]',
    re.DOTALL
)

# Matches a method signature following a [SpecCapability] attribute.
# Robust: skips XML doc comments, other attributes, #region/#endregion.
# Handles generic return types like Task<decimal>, expression-bodied members.
METHOD_SIG_RE = re.compile(
    r'\[SpecCapability\s*\(.*?\)\s*\]'
    r'(?:\s*(?:///[^\n]*|//[^\n]*|\#(?:region|endregion)[^\n]*|\[(?!SpecCapability)[^\]]*\]))*'
    r'\s*public\s+(?:static\s+|virtual\s+|override\s+|async\s+)*'
    r'([\w<>,\s]+?)\s+(\w+)\s*\(([^)]*)\)',
    re.DOTALL
)

# Matches any public method signature (for coverage report)
PUBLIC_METHOD_RE = re.compile(
    r'public\s+(?:static\s+|virtual\s+|override\s+|async\s+)*'
    r'[\w<>,\s]+?\s+(\w+)\s*\([^)]*\)\s*(?:\{|=>)',
    re.DOTALL
)


def parse_spec_option(code):
    """Extract SpecOption fields and class name from a C# code string.

    Returns a dict with parsed fields, or None if no [SpecOption] found.
    """
    match = SPEC_OPTION_RE.search(code)
    if not match:
        return None

    body = match.group(1)
    fields = {}
    for param_match in PARAM_RE.finditer(body):
        fields[param_match.group(1)] = param_match.group(2)

    if 'Category' not in fields or 'Name' not in fields:
        return None

    class_match = CLASS_NAME_RE.search(code)
    if class_match:
        fields['CodeClass'] = class_match.group(1)

    return fields


def to_snake_id(name):
    """Convert a PascalCase or space-separated name to a snake_case id."""
    # Insert underscore between a run of uppercase and an uppercase followed by lowercase (e.g. GMPEqualiser -> GMP_Equaliser)
    s = re.sub(r'([A-Z]+)([A-Z][a-z])', r'\1_\2', name)
    # Insert underscore between lowercase/digit and uppercase (e.g. capped -> capped_R)
    s = re.sub(r'([a-z0-9])([A-Z])', r'\1_\2', s)
    # Replace spaces/hyphens with underscores
    s = re.sub(r'[\s\-]+', '_', s)
    return s.lower()


def load_modules_from_dir(dir_path, verbose=False):
    """Walk a directory of .cs files and return module dicts (same shape as JSON input).

    Scheme is derived from the immediate subdirectory name.
    lastModified is taken from file OS mtime.
    """
    modules = []
    dir_path = os.path.abspath(dir_path)

    for root, dirs, files in os.walk(dir_path):
        # Sort for deterministic ordering
        dirs.sort()
        files.sort()

        for filename in files:
            if not filename.endswith('.cs'):
                continue

            filepath = os.path.join(root, filename)

            # Derive scheme from the first subdirectory under dir_path
            rel_path = os.path.relpath(filepath, dir_path)
            parts = rel_path.split(os.sep)
            scheme = parts[0] if len(parts) > 1 else ''

            module_name = os.path.splitext(filename)[0]

            with open(filepath, 'r') as f:
                code = f.read()

            mtime = os.path.getmtime(filepath)
            last_modified = datetime.fromtimestamp(mtime).strftime('%Y-%m-%dT%H:%M:%S')

            modules.append({
                'moduleName': module_name,
                'scheme': scheme,
                'code': code,
                'lastModified': last_modified,
            })

            if verbose:
                print(f"  READ {rel_path} (scheme={scheme})")

    return modules


def process_modules(modules, verbose=False):
    """Process a list of module dicts and return options grouped by category."""
    categories = {}

    for module in modules:
        module_name = module.get('moduleName', '<unknown>')
        code = module.get('code', '')

        if not code:
            if verbose:
                print(f"  SKIP {module_name}: no code")
            continue

        fields = parse_spec_option(code)
        if not fields:
            if verbose:
                print(f"  SKIP {module_name}: no [SpecOption] attribute")
            continue

        category = fields['Category'].lower()
        # Pluralise 'builder' -> 'builders' to match expected convention
        if category == 'builder':
            category = 'builders'

        last_modified = module.get('lastModified', '')
        if last_modified and 'T' in last_modified:
            last_modified = last_modified.split('T')[0]

        option = {
            'id': to_snake_id(fields.get('CodeClass', module_name)),
            'name': fields['Name'],
            'description': fields.get('Description', ''),
            'codeClass': fields.get('CodeClass', module_name),
            'scheme': module.get('scheme', ''),
            'lastModified': last_modified,
        }

        if 'WhyItMatters' in fields:
            option['whyItMatters'] = fields['WhyItMatters']

        categories.setdefault(category, []).append(option)

        if verbose:
            print(f"  FOUND {module_name} -> {category}/{option['id']}")

    return categories


def generate_js(categories):
    """Generate a JS string defining CODE_OPTIONS from grouped categories."""
    lines = ['const CODE_OPTIONS = {']

    sorted_cats = sorted(categories.keys())
    for i, cat in enumerate(sorted_cats):
        options = categories[cat]
        lines.append(f'    {cat}: [')

        for j, opt in enumerate(options):
            lines.append('        {')
            lines.append(f'            id: "{opt["id"]}",')
            lines.append(f'            name: "{opt["name"]}",')
            lines.append(f'            description: "{opt["description"]}",')
            if 'whyItMatters' in opt:
                lines.append(f'            whyItMatters: "{opt["whyItMatters"]}",')
            lines.append(f'            codeClass: "{opt["codeClass"]}",')
            lines.append(f'            scheme: "{opt["scheme"]}",')
            lines.append(f'            lastModified: "{opt["lastModified"]}"')
            trailing = ',' if j < len(options) - 1 else ''
            lines.append(f'        }}{trailing}')

        trailing = ',' if i < len(sorted_cats) - 1 else ''
        lines.append(f'    ]{trailing}')

    lines.append('};')
    lines.append('')
    return '\n'.join(lines)


def parse_spec_capabilities(code):
    """Extract all [SpecCapability] decorated methods from a C# code string.

    Returns a list of dicts, one per decorated method. Each dict includes
    attribute fields plus auto-extracted methodName, returnType, parameters.
    Returns an empty list if no [SpecCapability] found.
    """
    results = []
    for match in SPEC_CAPABILITY_RE.finditer(code):
        body = match.group(1)
        fields = {}
        for param_match in PARAM_RE.finditer(body):
            fields[param_match.group(1)] = param_match.group(2)

        if 'Category' not in fields or 'Name' not in fields:
            continue

        # Find the method signature that follows this specific attribute
        # Search from the start of this attribute match
        sig_search = METHOD_SIG_RE.search(code, match.start())
        if sig_search:
            fields['returnType'] = sig_search.group(1).strip()
            fields['methodName'] = sig_search.group(2)
            fields['parameters'] = sig_search.group(3).strip()

        results.append(fields)

    return results


def process_capabilities(modules, verbose=False):
    """Process a list of module dicts and return capabilities grouped by category."""
    categories = {}

    for module in modules:
        module_name = module.get('moduleName', '<unknown>')
        code = module.get('code', '')

        if not code:
            if verbose:
                print(f"  SKIP {module_name}: no code")
            continue

        capabilities = parse_spec_capabilities(code)
        if not capabilities:
            if verbose:
                print(f"  SKIP {module_name}: no [SpecCapability] attribute")
            continue

        # Extract class name for codeClass field
        class_match = re.search(r'public\s+class\s+(\w+)', code)
        code_class = class_match.group(1) if class_match else module_name

        # Check for a parent [SpecOption] on the same class
        parent_option = parse_spec_option(code)
        parent_info = None
        if parent_option and 'Category' in parent_option and 'Name' in parent_option:
            parent_info = {
                'id': to_snake_id(parent_option.get('CodeClass', module_name)),
                'name': parent_option['Name'],
            }

        last_modified = module.get('lastModified', '')
        if last_modified and 'T' in last_modified:
            last_modified = last_modified.split('T')[0]

        for fields in capabilities:
            category = fields['Category'].lower()

            capability = {
                'id': to_snake_id(fields.get('methodName', fields['Name'])),
                'name': fields['Name'],
                'description': fields.get('Description', ''),
                'codeClass': code_class,
                'scheme': module.get('scheme', ''),
                'lastModified': last_modified,
            }

            if 'WhyItMatters' in fields:
                capability['whyItMatters'] = fields['WhyItMatters']

            if 'methodName' in fields:
                capability['methodName'] = fields['methodName']
            if 'returnType' in fields:
                capability['returnType'] = fields['returnType']
            if 'parameters' in fields:
                capability['parameters'] = fields['parameters']

            if parent_info:
                capability['parentOption'] = parent_info

            categories.setdefault(category, []).append(capability)

            if verbose:
                print(f"  FOUND capability {module_name} -> {category}/{capability['id']}")

    return categories


def generate_capabilities_js(categories):
    """Generate a JS string defining CODE_CAPABILITIES from grouped categories."""
    lines = ['const CODE_CAPABILITIES = {']

    sorted_cats = sorted(categories.keys())
    for i, cat in enumerate(sorted_cats):
        caps = categories[cat]
        lines.append(f'    {cat}: [')

        for j, cap in enumerate(caps):
            lines.append('        {')
            lines.append(f'            id: "{cap["id"]}",')
            lines.append(f'            name: "{cap["name"]}",')
            lines.append(f'            description: "{cap["description"]}",')
            if 'whyItMatters' in cap:
                lines.append(f'            whyItMatters: "{cap["whyItMatters"]}",')
            if 'methodName' in cap:
                lines.append(f'            methodName: "{cap["methodName"]}",')
            if 'returnType' in cap:
                lines.append(f'            returnType: "{cap["returnType"]}",')
            if 'parameters' in cap:
                lines.append(f'            parameters: "{cap["parameters"]}",')
            if 'parentOption' in cap:
                lines.append(f'            parentOption: {{ id: "{cap["parentOption"]["id"]}", name: "{cap["parentOption"]["name"]}" }},')
            lines.append(f'            codeClass: "{cap["codeClass"]}",')
            lines.append(f'            scheme: "{cap["scheme"]}",')
            lines.append(f'            lastModified: "{cap["lastModified"]}"')
            trailing = ',' if j < len(caps) - 1 else ''
            lines.append(f'        }}{trailing}')

        trailing = ',' if i < len(sorted_cats) - 1 else ''
        lines.append(f'    ]{trailing}')

    lines.append('};')
    lines.append('')
    return '\n'.join(lines)


def generate_coverage_report(modules, verbose=False):
    """Generate a coverage report showing public methods vs [SpecCapability] methods per class.

    Only reports on classes that have a [SpecOption] attribute.
    Returns the report as a string.
    """
    lines = []
    total_documented = 0
    total_public = 0
    class_count = 0

    for module in modules:
        code = module.get('code', '')
        if not code:
            continue

        # Only report on classes with [SpecOption]
        option = parse_spec_option(code)
        if not option:
            continue

        class_name = option.get('CodeClass', module.get('moduleName', '<unknown>'))

        # Count public methods (exclude constructors — method name != class name)
        public_methods = []
        for m in PUBLIC_METHOD_RE.finditer(code):
            method_name = m.group(1)
            if method_name != class_name:
                public_methods.append(method_name)

        # Count [SpecCapability] decorated methods
        capabilities = parse_spec_capabilities(code)
        documented = len(capabilities)
        total = len(public_methods)

        total_documented += documented
        total_public += total
        class_count += 1

        pct = f'{documented / total * 100:.0f}%' if total > 0 else 'n/a'
        lines.append(f'  {class_name}: {documented}/{total} methods documented ({pct})')

    if class_count == 0:
        return 'Coverage Report:\n  No classes with [SpecOption] found.\n'

    overall_pct = f'{total_documented / total_public * 100:.0f}%' if total_public > 0 else 'n/a'
    lines.append('  ---')
    lines.append(f'  Overall: {total_documented}/{total_public} methods documented ({overall_pct})')

    return 'Coverage Report:\n' + '\n'.join(lines) + '\n'


def format_category(key):
    """Format a category key for display, preserving known acronyms."""
    known = {'gmp', 'dc', 'pcls', 'erf', 'lrf', 'afr', 'cetv'}
    words = key.replace('_', ' ').split()
    return ' '.join(w.upper() if w.lower() in known else w.title() for w in words)


def generate_preview(categories, cap_categories):
    """Generate a compact human-readable summary of extracted options and capabilities."""
    lines = []

    # Options
    total_options = sum(len(opts) for opts in categories.values())
    lines.append(f'Options ({total_options}):')
    if total_options == 0:
        lines.append('  (none)')
    for cat in sorted(categories.keys()):
        opts = categories[cat]
        items = ', '.join(f'{o["name"]} ({o["codeClass"]})' for o in opts)
        lines.append(f'  {format_category(cat):14s} {items}')

    lines.append('')

    # Capabilities — group by parent
    total_caps = sum(len(caps) for caps in cap_categories.values())
    lines.append(f'Capabilities ({total_caps}):')
    if total_caps == 0:
        lines.append('  (none)')
    for cat in sorted(cap_categories.keys()):
        caps = cap_categories[cat]
        # Split into parented and standalone
        parented = {}
        standalone = []
        for c in caps:
            parent = c.get('parentOption')
            if parent:
                parented.setdefault(parent['name'], []).append(c['name'])
            else:
                standalone.append(c['name'])

        parts = []
        for parent_name, cap_names in parented.items():
            parts.append(f'{", ".join(cap_names)}  -> parent: {parent_name}')
        if standalone:
            parts.append(f'{", ".join(standalone)}  (standalone)')

        lines.append(f'  {format_category(cat):14s} {"; ".join(parts)}')

    lines.append('')
    return '\n'.join(lines)


def main():
    parser = argparse.ArgumentParser(
        description='Extract [SpecOption] and [SpecCapability] attributes from C# files and generate JS files.'
    )
    parser.add_argument('input', help='Directory of .cs files, or JSON file (with --json)')
    parser.add_argument('--json', action='store_true',
                        help='Treat input as a JSON file (backward compatibility mode)')
    parser.add_argument('-o', '--output', default='./code-options.js',
                        help='Output JS file for spec options (default: ./code-options.js)')
    parser.add_argument('-c', '--capabilities-output', default='./code-capabilities.js',
                        help='Output JS file for spec capabilities (default: ./code-capabilities.js)')
    parser.add_argument('--preview', action='store_true',
                        help='Print a formatted summary of extracted options and capabilities')
    parser.add_argument('--coverage', action='store_true',
                        help='Print a coverage report showing documented vs total public methods')
    parser.add_argument('-v', '--verbose', action='store_true',
                        help='Print diagnostic output')
    args = parser.parse_args()

    # --- Load modules ---
    if args.json:
        with open(args.input, 'r') as f:
            modules = json.load(f)
        if args.verbose:
            print(f"Loaded {len(modules)} modules from {args.input} (JSON mode)")
    else:
        if not os.path.isdir(args.input):
            print(f"Error: {args.input} is not a directory. Use --json for JSON input.", file=sys.stderr)
            sys.exit(1)
        if args.verbose:
            print(f"Scanning {args.input} for .cs files...")
        modules = load_modules_from_dir(args.input, verbose=args.verbose)
        if args.verbose:
            print(f"Loaded {len(modules)} .cs files from {args.input}")

    # --- Spec Options pipeline ---
    if args.verbose:
        print("\n--- Spec Options ---")
    categories = process_modules(modules, verbose=args.verbose)

    total_options = sum(len(opts) for opts in categories.values())
    if args.verbose:
        print(f"\nExtracted {total_options} spec options across {len(categories)} categories")
        for cat, opts in sorted(categories.items()):
            print(f"  {cat}: {len(opts)} options")

    js_output = generate_js(categories)

    with open(args.output, 'w') as f:
        f.write(js_output)

    if args.verbose:
        print(f"Wrote {args.output}")

    # --- Spec Capabilities pipeline ---
    if args.verbose:
        print("\n--- Spec Capabilities ---")
    cap_categories = process_capabilities(modules, verbose=args.verbose)

    total_caps = sum(len(caps) for caps in cap_categories.values())
    if args.verbose:
        print(f"\nExtracted {total_caps} spec capabilities across {len(cap_categories)} categories")
        for cat, caps in sorted(cap_categories.items()):
            print(f"  {cat}: {len(caps)} capabilities")

    caps_js_output = generate_capabilities_js(cap_categories)

    with open(args.capabilities_output, 'w') as f:
        f.write(caps_js_output)

    if args.verbose:
        print(f"Wrote {args.capabilities_output}")

    # --- Preview ---
    if args.preview:
        if args.verbose:
            print()
        print(generate_preview(categories, cap_categories))

    # --- Coverage report ---
    if args.coverage:
        if args.verbose:
            print()
        report = generate_coverage_report(modules, verbose=args.verbose)
        print(report)

    if not args.verbose and not args.coverage and not args.preview:
        print(f"Extracted {total_options} spec options -> {args.output}")
        print(f"Extracted {total_caps} spec capabilities -> {args.capabilities_output}")


if __name__ == '__main__':
    main()
