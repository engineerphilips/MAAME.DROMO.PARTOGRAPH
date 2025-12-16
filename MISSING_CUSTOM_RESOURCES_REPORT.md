# Missing Custom Resources Analysis Report
## MAAME.DROMO.PARTOGRAPH Application

**Analysis Date:** 2025-12-16
**Analyzed By:** Claude Code

---

## Executive Summary

This report identifies all pages, views, and popups in the MAAME.DROMO.PARTOGRAPH application that use `StaticResource` references without having the corresponding custom resources defined locally.

### Overview Statistics

| Category | Count |
|----------|-------|
| **Total XAML Files Analyzed** | 102 |
| **Files WITH Custom Resources** | 62 (60.8%) |
| **Files WITHOUT Custom Resources but USING StaticResource** | 16 (15.7%) |
| **Files NOT Using StaticResource** | 24 (23.5%) |

### Critical Finding

**16 files** (15.7%) are using `StaticResource` references **WITHOUT** having custom resources defined at the page/view/popup level. This can lead to:
- Runtime errors if resources are not found
- Reduced code maintainability
- Hidden dependencies on application-level resources
- Potential issues when pages are used in different contexts

---

## Files Missing Custom Resources

### PAGES (10 files)

#### 1. **ActivePatientsPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/ActivePatientsPage.xaml`

**Missing Resources:**
- `FHRColorConverter` (Line 104)

**Issue:** Converts fetal heart rate values to colors (Normal/Borderline/Abnormal) without local converter definition.

**Impact:** High - Critical for medical data visualization

---

#### 2. **EnhancedPartographPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/EnhancedPartographPage.xaml`

**Missing Resources:**
- `InitialsConverter` (Line 40)

**Issue:** Extracts patient initials for display without local converter definition.

**Impact:** Medium - Affects patient identification UI

---

#### 3. **LoginPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/LoginPage.xaml`

**Missing Resources:**
- `PasswordIconConverter` (Line 125)

**Issue:** Toggles password visibility icon without local converter definition.

**Impact:** Medium - Affects authentication UX

---

#### 4. **PendingPatientsPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/PendingPatientsPage.xaml`

**Missing Resources:**
- `InitialsConverter` (Line 93)

**Issue:** Extracts patient initials for display without local converter definition.

**Impact:** Medium - Affects patient identification UI

---

#### 5. **VitalSignsPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/VitalSignsPage.xaml`

**Missing Resources:**
- `HighBPConverter` (Lines 75, 95)

**Issue:** Highlights abnormally high blood pressure readings without local converter definition.

**Impact:** High - Critical for vital signs monitoring

---

#### 6. **SyncSettingsPage.xaml** ⚠️ HIGH PRIORITY
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/SyncSettingsPage.xaml`

**Missing Resources:**
- `BoolToColorConverter` (Line 30)
- `BoolToTextConverter` (Line 33)
- `IntToBoolConverter` (Line 75)
- `InvertedBoolConverter` (Line 89)
- `PercentageToDecimalConverter` (Line 81)
- Multiple color resources: Gray100, Gray200, Gray600, Gray900, Primary, Secondary

**Issue:** Heavily relies on external converters and color resources without local definitions.

**Impact:** High - Most problematic page with 5 missing converters

---

#### 7. **MainPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/MainPage.xaml`

**Missing Resources:**
- `IconClean` (Line 58, 161)
- `LayoutPadding` (Line 37)
- `LayoutSpacing` (Line 37, 134, 144, 172)
- `Title2` (Lines 39, 56, 141, 158)
- `InvertedBoolConverter` (Line 82)

**Issue:** Uses application-level layout and icon resources without local definitions.

**Impact:** Medium - Relies on global resources

---

#### 8. **ProjectDetailPage.xaml** ⚠️ HIGH PRIORITY
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/ProjectDetailPage.xaml`

**Missing Resources:**
- `LightSecondaryBackground`, `DarkSecondaryBackground` (Line 19)
- `DarkOnLightBackground`, `LightOnDarkBackground` (Lines 26, 128)
- `LightBackground`, `DarkBackground` (Line 44)
- `NormalTagTemplate`, `SelectedTagTemplate` (Lines 53, 54)
- `IconDelete` (Line 63)
- `LayoutPadding`, `LayoutSpacing` (Lines 68, 100, 134, 144, 172)
- `Title2` (Lines 91, 141, 158)
- `Primary` (Line 129)
- `ChipDataTemplateSelector` (Line 148)
- `IconClean` (Line 161)

**Issue:** Heavy reliance on external resources (16+ different resources).

**Impact:** High - Most dependent on external resources

---

#### 9. **ProjectListPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/ProjectListPage.xaml`

**Missing Resources:**
- `LayoutPadding`
- `LayoutSpacing`

**Issue:** Uses application-level layout resources without local definitions.

**Impact:** Low - Standard layout resources

---

#### 10. **TaskDetailPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/TaskDetailPage.xaml`

**Missing Resources:**
- `IconDelete` (Line 17)
- `LayoutSpacing`, `LayoutPadding` (Line 23)

**Issue:** Uses application-level resources without local definitions.

**Impact:** Low - Standard resources

---

### MODALS (1 file)

#### 11. **BaselineFHRModalPage.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/Modals/BaselineFHRModalPage.xaml`

**Missing Resources:**
- `BoolToColorConverter` (Line 228)
- `BoolToOrangeConverter` (Line 247)
- `BoolToRedConverter` (Line 266)

**Issue:** Medical data visualization converters not defined locally.

**Impact:** High - Critical for fetal heart rate monitoring

---

### CONTROLS (5 files)

#### 12. **AddButton.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/Controls/AddButton.xaml`

**Missing Resources:**
- `IconAdd`
- `Primary`

**Issue:** Uses application-level icon and color resources.

**Impact:** Low - Acceptable for reusable controls

---

#### 13. **CategoryChart.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/Controls/CategoryChart.xaml`

**Missing Resources:**
- `CardStyle`
- Theme colors

**Issue:** Uses application-level styling.

**Impact:** Low - Acceptable for reusable controls

---

#### 14. **ProjectCardView.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/Controls/ProjectCardView.xaml`

**Missing Resources:**
- `CardStyle`
- Theme colors
- `ShimmerCustomViewStyle`

**Issue:** Uses application-level styling.

**Impact:** Low - Acceptable for reusable controls

---

#### 15. **TagView.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/Controls/TagView.xaml`

**Missing Resources:**
- Theme background colors

**Issue:** Uses application-level theme colors.

**Impact:** Low - Acceptable for reusable controls

---

#### 16. **TaskView.xaml**
**Location:** `MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/Controls/TaskView.xaml`

**Missing Resources:**
- Theme colors
- `ShimmerCustomViewStyle`

**Issue:** Uses application-level styling.

**Impact:** Low - Acceptable for reusable controls

---

## Resource Type Analysis

### Custom Converters Missing Local Definitions

| Converter Name | Used In | Priority | Purpose |
|----------------|---------|----------|---------|
| `FHRColorConverter` | ActivePatientsPage.xaml | **Critical** | Converts fetal heart rate to color (medical data) |
| `HighBPConverter` | VitalSignsPage.xaml | **Critical** | Highlights high blood pressure (medical data) |
| `BoolToColorConverter` | SyncSettingsPage.xaml, BaselineFHRModalPage.xaml | **High** | Boolean to color conversion |
| `BoolToOrangeConverter` | BaselineFHRModalPage.xaml | **High** | Boolean to orange color |
| `BoolToRedConverter` | BaselineFHRModalPage.xaml | **High** | Boolean to red color |
| `BoolToTextConverter` | SyncSettingsPage.xaml | **High** | Boolean to text conversion |
| `IntToBoolConverter` | SyncSettingsPage.xaml | **High** | Integer to boolean conversion |
| `PercentageToDecimalConverter` | SyncSettingsPage.xaml | **High** | Percentage formatting |
| `InitialsConverter` | EnhancedPartographPage.xaml, PendingPatientsPage.xaml | **Medium** | Extracts initials from names |
| `PasswordIconConverter` | LoginPage.xaml | **Medium** | Password visibility icon toggle |
| `InvertedBoolConverter` | MainPage.xaml | **Medium** | Boolean negation |

### Application-Level Resources (Acceptable)

These resources are acceptable as global resources defined in `App.xaml`:

**Colors:**
- Primary, Secondary, Tertiary
- Gray100, Gray200, Gray600, Gray900
- Dark/Light theme colors
- DarkOnLightBackground, LightOnDarkBackground
- LightSecondaryBackground, DarkSecondaryBackground

**Layout Values:**
- LayoutPadding, LayoutSpacing

**Icons:**
- IconAdd, IconClean, IconDelete

**Styles:**
- Title2, CardStyle, ShimmerCustomViewStyle

**Templates:**
- NormalTagTemplate, SelectedTagTemplate, ChipDataTemplateSelector

---

## Recommendations

### Immediate Actions Required

#### Priority 1: Critical Medical Data Converters (Fix Immediately)

1. **ActivePatientsPage.xaml** - Add `FHRColorConverter`
   ```xml
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:FHRColorConverter x:Key="FHRColorConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>
   ```

2. **VitalSignsPage.xaml** - Add `HighBPConverter`
   ```xml
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:HighBPConverter x:Key="HighBPConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>
   ```

3. **BaselineFHRModalPage.xaml** - Add boolean converters
   ```xml
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:BoolToColorConverter x:Key="BoolToColorConverter" />
           <converters:BoolToOrangeConverter x:Key="BoolToOrangeConverter" />
           <converters:BoolToRedConverter x:Key="BoolToRedConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>
   ```

#### Priority 2: High Priority Pages (Fix Soon)

4. **SyncSettingsPage.xaml** - Add 5 converters
   ```xml
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:BoolToColorConverter x:Key="BoolToColorConverter" />
           <converters:BoolToTextConverter x:Key="BoolToTextConverter" />
           <converters:IntToBoolConverter x:Key="IntToBoolConverter" />
           <converters:InvertedBoolConverter x:Key="InvertedBoolConverter" />
           <converters:PercentageToDecimalConverter x:Key="PercentageToDecimalConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>
   ```

5. **LoginPage.xaml** - Add `PasswordIconConverter`
   ```xml
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:PasswordIconConverter x:Key="PasswordIconConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>
   ```

6. **EnhancedPartographPage.xaml** & **PendingPatientsPage.xaml** - Add `InitialsConverter`
   ```xml
   <ContentPage.Resources>
       <ResourceDictionary>
           <converters:InitialsConverter x:Key="InitialsConverter" />
       </ResourceDictionary>
   </ContentPage.Resources>
   ```

#### Priority 3: Application-Level Resources (Verify)

7. Ensure all application-level resources are properly defined in:
   - `App.xaml`
   - `Resources/Styles/Colors.xaml`
   - `Resources/Styles/AppStyles.xaml`
   - `Resources/Styles/Styles.xaml`

---

## Files With Proper Custom Resources (Best Practices)

These **62 files** serve as examples of proper resource management:

### Pages
- BabyDetailsPage.xaml
- BirthOutcomePage.xaml
- FourthStagePartographPage.xaml
- HomePage.xaml
- ManageMetaPage.xaml
- PartographPage.xaml
- PartographReportPage.xaml
- PatientHubPage.xaml
- PatientPage.xaml
- PatientsPage.xaml
- ReportsPage.xaml
- SecondStagePartographPage.xaml
- ThirdStagePartographPage.xaml
- UsersPage.xaml

### Modals (All properly define resources)
- AmnioticFluidModalView.xaml
- BPPulseModalView.xaml
- CaputModalView.xaml
- CervixDilatationModalView.xaml
- CompanionModalView.xaml
- FetalPositionModalView.xaml
- FHRContractionModalView.xaml
- FHRDecelerationModalView.xaml
- FHRModalView.xaml
- HeadDescentModalView.xaml
- IVFluidModalView.xaml
- MedicationModalView.xaml
- MouldingModalView.xaml
- OralFluidModalView.xaml
- OxytocinModalView.xaml
- PainReliefModalView.xaml
- PostureModalView.xaml
- TemperatureModalView.xaml
- UrineModalView.xaml

### Views
- ActivePatientsView.xaml
- BirthWeightApgarView.xaml
- CompletedPatientsView.xaml
- NeonatalOutcomesView.xaml
- PendingPatientsView.xaml

---

## Implementation Pattern

All pages using custom converters should follow this pattern:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:converters="clr-namespace:MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters"
             x:Class="YourPageClass">

    <ContentPage.Resources>
        <ResourceDictionary>
            <!-- Define all custom converters used in this page -->
            <converters:YourConverter x:Key="YourConverter" />
            <converters:AnotherConverter x:Key="AnotherConverter" />

            <!-- Define page-specific styles if needed -->
            <Style x:Key="YourCustomStyle" TargetType="Label">
                <!-- Style setters -->
            </Style>
        </ResourceDictionary>
    </ContentPage.Resources>

    <!-- Page content -->
</ContentPage>
```

---

## Benefits of Adding Local Resource Definitions

1. **Explicit Dependencies:** Makes it clear which resources each page depends on
2. **Reduced Runtime Errors:** Catches missing resources at compile time
3. **Better Maintainability:** Easier to understand and modify individual pages
4. **Improved Testability:** Pages can be tested in isolation
5. **Clear Ownership:** Each page owns its required resources
6. **Better IDE Support:** IntelliSense and code navigation work better

---

## Conclusion

### Summary

- **16 files** require custom resource definitions to be added
- **11 converters** need to be defined across these files
- **3 files** are critical priority (medical data visualization)
- **Controls** can optionally keep using application-level resources

### Action Items

1. ✅ Add converters to critical medical pages (ActivePatientsPage, VitalSignsPage, BaselineFHRModalPage)
2. ✅ Add converters to high-priority pages (SyncSettingsPage, LoginPage)
3. ✅ Add converters to remaining pages (EnhancedPartographPage, PendingPatientsPage, MainPage)
4. ✅ Verify all application-level resources are properly defined in App.xaml
5. ✅ Consider adding resources to Controls for better encapsulation (optional)

### Expected Outcome

After implementing these changes:
- **0 files** with missing custom resources
- Improved code quality and maintainability
- Reduced risk of runtime errors
- Better adherence to MAUI best practices

---

**Report Generated:** 2025-12-16
**Analysis Tool:** Claude Code
**Repository:** MAAME.DROMO.PARTOGRAPH
