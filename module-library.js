/**
 * Module Library Data for Pension Calc Specbuilder
 *
 * Edit this file to add/remove/modify the available options.
 * Each item has plain-English descriptions for non-technical users.
 */

const MODULE_LIBRARY = {

    // ─── CALCULATION TYPES (the "main course") ───────────────────────
    calcTypes: [
        {
            id: 'dtor',
            name: 'Deferred to Retirement',
            shortName: 'Def > Ret',
            description: 'Calculates pension benefits when a deferred member reaches retirement age. The most common calculation type.',
            whyItMatters: 'This is the bread-and-butter calculation. Most schemes need this first.',
            icon: 'calendar-check'
        },
        {
            id: 'ator',
            name: 'Active to Retirement',
            shortName: 'Act > Ret',
            description: 'Calculates pension benefits for an active (still working) member who is retiring. Includes final salary or CARE accrual up to retirement date.',
            whyItMatters: 'Needed when members retire directly from active service, without a deferred period.',
            icon: 'briefcase-clock'
        },
        {
            id: 'transfer_out',
            name: 'Transfer Out (CETV)',
            shortName: 'Transfer Out',
            description: 'Calculates the Cash Equivalent Transfer Value — the lump sum a member can take to another pension scheme.',
            whyItMatters: 'Required by law when a member requests a transfer. Has strict regulatory timelines.',
            icon: 'arrow-right-from-bracket'
        },
        {
            id: 'death_before_ret',
            name: 'Death Before Retirement',
            shortName: 'Death (pre-ret)',
            description: 'Calculates benefits payable when a member dies before reaching retirement — typically a spouse pension and/or lump sum.',
            whyItMatters: 'Sensitive calculation. Needs to handle spouse pensions, lump sum death benefits, and expression of wish.',
            icon: 'heart-pulse'
        },
        {
            id: 'death_after_ret',
            name: 'Death After Retirement',
            shortName: 'Death (post-ret)',
            description: 'Calculates the ongoing spouse/dependant pension when a pensioner dies. May include a guarantee period lump sum.',
            whyItMatters: 'Simpler than pre-retirement death but still needs spouse fractions and GMP handling.',
            icon: 'heart'
        },
        {
            id: 'trivial_commutation',
            name: 'Trivial Commutation',
            shortName: 'Trivial Comm.',
            description: 'Converts a small pension into a one-off lump sum when the total value is below the trivial commutation limit.',
            whyItMatters: 'Useful for clearing small pots. The limit changes each tax year.',
            icon: 'coins'
        },
        {
            id: 'pension_sharing',
            name: 'Pension Sharing on Divorce',
            shortName: 'Divorce PSO',
            description: 'Implements a Pension Sharing Order — splits pension benefits between the member and ex-spouse after divorce.',
            whyItMatters: 'Legally mandated when a court issues a PSO. Complex because it creates a new "pension credit" member.',
            icon: 'scale-balanced'
        },
        {
            id: 'estimate',
            name: 'Retirement Estimate',
            shortName: 'Estimate',
            description: 'Provides an illustration of projected benefits at a future date. Not a firm quotation — uses assumptions about future service/revaluation.',
            whyItMatters: 'Members request these to help plan their retirement. Less precision needed than a firm quote.',
            icon: 'chart-line'
        }
    ],

    // ─── SERVICE TRANCHE OPTIONS ─────────────────────────────────────
    // These are the dropdown/selection options available when configuring each tranche

    benefitTypes: [
        { id: 'final_salary', name: 'Final Salary', description: 'Pension based on salary at leaving/retirement multiplied by years of service and an accrual rate.' },
        { id: 'care', name: 'CARE (Career Average)', description: 'Each year\'s pension accrual is based on that year\'s salary, then revalued to retirement.' },
        { id: 'flat_rate', name: 'Flat Rate', description: 'A fixed amount of pension per year of service, not linked to salary.' }
    ],

    accrualRates: [
        { id: '1_60', name: '1/60th', description: 'Common for older scheme rules. Each year of service earns 1/60th of final salary.' },
        { id: '1_80', name: '1/80th', description: 'Common post-reform. Each year of service earns 1/80th of final salary (often with a separate lump sum).' },
        { id: '1_100', name: '1/100th', description: 'Lower accrual rate, sometimes used for post-reform tranches.' },
        { id: '1_120', name: '1/120th', description: 'Used in some reformed scheme sections.' },
        { id: 'other', name: 'Other', description: 'A non-standard accrual rate — specify in the notes.' }
    ],

    revaluationMethods: [
        {
            id: 's52',
            name: 'Statutory (s52)',
            description: 'Compound revaluation at the statutory rate. Uses published revaluation orders for past years and an assumed future rate (AFR) for future years.',
            whyItMatters: 'The default for pre-97 deferred benefits. The AFR assumption matters — check what the scheme actuary recommends.',
            category: 'Statutory'
        },
        {
            id: 's101',
            name: 'CPI-Capped (s101)',
            description: 'Statutory revaluation capped at CPI (previously RPI). Uses published revaluation factors and an AFR for future years.',
            whyItMatters: 'The standard for post-97 deferred benefits. The cap means it\'s usually lower than s52.',
            category: 'Statutory'
        },
        {
            id: 'fixed',
            name: 'Fixed Rate',
            description: 'Revaluation at a fixed percentage per year (e.g. 5%, 2.5%). No dependency on published factors.',
            whyItMatters: 'Simple to implement. Some older schemes used fixed rates rather than statutory revaluation.',
            category: 'Fixed'
        },
        {
            id: 'no_reval',
            name: 'No Revaluation',
            description: 'Benefits are frozen at the leaving value. No growth between leaving and retirement.',
            whyItMatters: 'Rare, but some benefits (like trivial amounts or certain AVCs) don\'t revalue.',
            category: 'Fixed'
        },
        {
            id: 'care_reval',
            name: 'CARE Revaluation (CPI / CPI+X%)',
            description: 'Each year\'s CARE accrual is revalued by CPI (or CPI + X%). Different to deferred revaluation — this is the in-scheme revaluation.',
            whyItMatters: 'CARE revaluation applies while still in service, not just after leaving. The rate may differ from deferred reval.',
            category: 'CARE'
        }
    ],

    retirementFactorTypes: [
        { id: 'erf', name: 'Early Retirement Factor (ERF)', description: 'Reduces the pension if the member retires before NRD.' },
        { id: 'lrf', name: 'Late Retirement Factor (LRF)', description: 'Increases the pension if the member retires after NRD.' },
        { id: 'none', name: 'No retirement factor', description: 'Benefits are paid unreduced regardless of retirement date.' }
    ],

    // ─── COMMUTATION OPTIONS ─────────────────────────────────────────
    commutationStyles: [
        {
            id: 'proportional',
            name: 'Proportional Commutation',
            description: 'All pension elements are commuted in the same proportion. If a member takes 20% cash, each element reduces by 20%.',
            whyItMatters: 'The simpler approach. Most schemes use this unless they have a specific commutation order.'
        },
        {
            id: 'ordered',
            name: 'Ordered Commutation',
            description: 'Pension elements are commuted in a specific order. The first element is fully commuted before moving to the next.',
            whyItMatters: 'More complex. The order usually starts with the element that has the lowest pension increase rate.'
        }
    ],

    commutationFactorSources: [
        { id: 'single_table', name: 'Single Factor Table', description: 'One commutation factor table applies to all tranches.' },
        { id: 'per_tranche', name: 'Factor Per Tranche', description: 'Different commutation factor tables for different tranches.' },
        { id: 'age_based', name: 'Age-Based Factors', description: 'Factors vary by age at retirement (looked up from a table).' }
    ],

    // ─── GMP HANDLING ────────────────────────────────────────────────
    gmpHandling: [
        {
            id: 'gmp_equalisation',
            name: 'GMP Equalisation',
            description: 'Adjusts benefits so that men and women receive equal treatment despite historically different GMP ages (60 for women, 65 for men).',
            whyItMatters: 'Following the Lloyds ruling, most schemes need to equalise for GMP. The method chosen has a significant impact on benefits.',
            complexity: 'high',
            methods: [
                { id: 'method_c2', name: 'Method C2', description: 'Compares male and female benefits year by year and pays the higher. The most common approach.' },
                { id: 'method_d', name: 'Method D', description: 'Converts GMP to scheme pension. Simpler but may produce different results.' },
                { id: 'dual_record', name: 'Dual Record', description: 'Maintains parallel male/female calculations throughout. Most accurate but most complex.' }
            ]
        },
        {
            id: 'gmp_missed_increases',
            name: 'GMP Missed Increases',
            description: 'Post-88 GMP accumulates "missed increases" between the date of leaving and retirement. These are increases the state would have paid.',
            whyItMatters: 'Only applies to post-88 GMP elements. The missed increases file must be uploaded to Calculate.',
            complexity: 'medium'
        },
        {
            id: 'gmp_restriction',
            name: 'GMP Restriction (Anti-Franking)',
            description: 'Ensures the total pension is at least as much as the GMP. If the scheme pension falls below GMP, it must be "topped up".',
            whyItMatters: 'Prevents the scheme from "franking" (reducing) the pension below the guaranteed minimum.',
            complexity: 'medium',
            options: [
                { id: 'at_retirement', name: 'At Retirement Date', description: 'Compare total pension to total GMP at the date of retirement.' },
                { id: 'at_gmp_date', name: 'At GMP Date', description: 'Compare total pension to total GMP at the member\'s GMP date (age 60/65).' }
            ]
        },
        {
            id: 'gmp_260_weeks',
            name: '260-Week Cap',
            description: 'Caps GMP revaluation at 260 weeks of earnings factor. An older rule that limits the maximum revalued GMP.',
            whyItMatters: 'Rarely hits in practice, but some schemes still need this check for compliance.',
            complexity: 'low'
        },
        {
            id: 'gmp_6aprils',
            name: 'GMP Reval Period',
            description: 'Choose whether GMP revaluation counts by "6 Aprils" or "complete tax years". A technical distinction that affects the factor lookup.',
            whyItMatters: 'Different schemes interpret the rules differently. Check with the scheme actuary.',
            complexity: 'low',
            options: [
                { id: 'six_aprils', name: '6 Aprils', description: 'Count the number of 6 Aprils between leaving and GMP date.' },
                { id: 'complete_tax_years', name: 'Complete Tax Years', description: 'Count complete tax years between leaving and GMP date.' }
            ]
        }
    ],

    // ─── GMP REVALUATION METHODS (for GMP elements within tranches) ──
    gmpRevalMethods: [
        { id: 'gmp_fixed', name: 'GMP Fixed Rate', description: 'GMP revalued at a fixed rate (usually 6% or 4.5% compound) from date of leaving to GMP age.' },
        { id: 'gmp_s148', name: 'GMP s148 Orders', description: 'GMP revalued using Section 148 orders (earnings factors). Published annually by the government.' },
        { id: 'gmp_limited', name: 'GMP Limited Rate', description: 'GMP revalued at the lower of a fixed rate and s148 orders.' }
    ],

    // ─── VALIDATIONS ─────────────────────────────────────────────────
    validations: {
        preCalc: [
            {
                id: 'valid_category',
                name: 'Valid Member Category',
                description: 'Check the member\'s category is in the list of valid categories for this calculation.',
                whyItMatters: 'Prevents running the wrong calculation for the wrong type of member.',
                isStandard: true
            },
            {
                id: 'factors_available',
                name: 'Retirement Factors Available',
                description: 'Check that ERF/LRF factor tables are loaded and have values for the member\'s retirement date.',
                whyItMatters: 'The calculation will produce wrong results if factors are missing. Better to stop early.',
                isStandard: true
            },
            {
                id: 'leaving_date',
                name: 'Date of Leaving Exists',
                description: 'Check the member has a date of leaving or date of termination of pensionable service.',
                whyItMatters: 'Revaluation can\'t be calculated without knowing when the member left.',
                isStandard: true
            },
            {
                id: 'nrd_exists',
                name: 'Normal Retirement Date Exists',
                description: 'Check the member has a Normal Retirement Date (NRD) on their record.',
                whyItMatters: 'ERF/LRF calculations compare retirement date to NRD. Without NRD, the calc fails.',
                isStandard: false
            },
            {
                id: 'retirement_date_valid',
                name: 'Retirement Date After Leaving',
                description: 'Check the requested retirement date is after the date of leaving.',
                whyItMatters: 'A basic sanity check. You can\'t retire before you\'ve left.',
                isStandard: false
            },
            {
                id: 'element_values_exist',
                name: 'Element Values Exist',
                description: 'Check the member has at least one deferred element value on their record.',
                whyItMatters: 'No point running the calculation if there are no benefits to calculate.',
                isStandard: false
            }
        ],
        postCalc: [
            {
                id: 'trivial_check',
                name: 'Trivial Commutation Check',
                description: 'Flag if the total capital value is below the trivial commutation limit (currently ~30,000).',
                whyItMatters: 'The member may be eligible for trivial commutation instead of a regular pension.',
                isStandard: true
            },
            {
                id: 'negative_cash',
                name: 'Negative Maximum Cash',
                description: 'Warn if the maximum tax-free cash comes out negative. This usually means GMP exceeds the scheme pension.',
                whyItMatters: 'Indicates GMP restriction is biting. The admin team needs to know.',
                isStandard: true
            },
            {
                id: 'avc_check',
                name: 'AVC/External Fund Check',
                description: 'Flag if external fund values (AVCs, DC funds) were found and included in the calculation.',
                whyItMatters: 'External funds may need separate handling or member communication.',
                isStandard: true
            },
            {
                id: 'gmp_exceeds_pension',
                name: 'GMP Exceeds Scheme Pension',
                description: 'Warn if the GMP is larger than the total scheme pension (after revaluation and retirement factors).',
                whyItMatters: 'This is a red flag — it may indicate a data issue or a very unusual case.',
                isStandard: false
            },
            {
                id: 'large_cash_check',
                name: 'Large Cash Sum Check',
                description: 'Flag if the cash sum exceeds HMRC limits or scheme-specific thresholds.',
                whyItMatters: 'Large cash sums may trigger tax charges. Worth flagging for manual review.',
                isStandard: false
            }
        ]
    },

    // ─── DEPENDENCIES (setup checklist for coders) ───────────────────
    dependencies: [
        {
            id: 'element_values_module',
            name: 'Element Values Module',
            description: 'The scheme module that reads element values and GMP element values from the member record.',
            whyItMatters: 'Without this module, the calculation can\'t read any pension data from the member.',
            category: 'Module'
        },
        {
            id: 'pcls_module',
            name: 'PCLS & Commutation Module',
            description: 'The scheme module that handles commutation — converting pension to tax-free cash.',
            whyItMatters: 'Needed for any calculation that involves a cash option (most retirement calcs).',
            category: 'Module'
        },
        {
            id: 'reusable_functions',
            name: 'Reusable Functions Module',
            description: 'Shared functions like termination date lookup, revaluation end date, and retirement factor builder.',
            whyItMatters: 'These functions are used across multiple calculation types. Must be published before the calc will compile.',
            category: 'Module'
        },
        {
            id: 'reval_base',
            name: 'Revaluation Base Dependency',
            description: 'The core revaluation engine that applies revaluation factors to deferred benefits.',
            whyItMatters: 'Handles the maths of compounding revaluation factors over the deferment period.',
            category: 'Dependency'
        },
        {
            id: 'reval_dependency',
            name: 'Revaluation Dependency',
            description: 'Links the calculation to the revaluation tables (s52, s101, etc.).',
            whyItMatters: 'Must be added as a project dependency so the calculation can access published revaluation factors.',
            category: 'Dependency'
        },
        {
            id: 'pension_option',
            name: 'Pension Option Dependency',
            description: 'The engine that builds the pension option — combining elements, applying commutation, and generating the output.',
            whyItMatters: 'Handles the final step: turning individual pension elements into a coherent set of member options.',
            category: 'Dependency'
        },
        {
            id: 'erf_table',
            name: 'ERF Table Upload',
            description: 'Upload the Early Retirement Factor table to Calculate (Factors > Decimal Factors).',
            whyItMatters: 'The calculation looks up ERFs by years-and-months early. Missing factors = calculation failure.',
            category: 'Data'
        },
        {
            id: 'lrf_table',
            name: 'LRF Table Upload',
            description: 'Upload the Late Retirement Factor table to Calculate (Factors > Decimal Factors).',
            whyItMatters: 'Only needed if the scheme has LRFs. Check section 4 of the spec.',
            category: 'Data'
        },
        {
            id: 'gmp_increases',
            name: 'GMP Increases File',
            description: 'Upload the GMP pension increases file to Calculate (Factors > Pension Increases).',
            whyItMatters: 'Needed for post-88 GMP missed increases. Without this, the GMP calculation will fail.',
            category: 'Data'
        },
        {
            id: 'afr_values',
            name: 'AFR Values Setup',
            description: 'Set the Assumed Future Revaluation rates in Calculate (Factors > Decimal Reference Values).',
            whyItMatters: 'Used for revaluation years beyond the latest published factors. Usually set by the scheme actuary.',
            category: 'Data'
        },
        {
            id: 'comm_factors',
            name: 'Commutation Factors Upload',
            description: 'Upload commutation factor tables to Calculate.',
            whyItMatters: 'Needed for any calc with a cash option. Factors convert pension into a lump sum.',
            category: 'Data'
        },
        {
            id: 's148_orders',
            name: 's148 Orders Upload',
            description: 'Upload Section 148 orders to Calculate for GMP revaluation.',
            whyItMatters: 'Only needed if the scheme uses s148 GMP revaluation (rather than fixed rate).',
            category: 'Data'
        }
    ]
};
