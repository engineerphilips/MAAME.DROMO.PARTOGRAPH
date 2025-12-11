# Partograph Second Stage Implementation

## Overview
This implementation adds comprehensive birth outcome recording, baby details tracking, and referral management to the partograph system, all based on WHO 2020 guidelines.

## What Has Been Implemented

### 1. Data Models

#### BirthOutcome Model
Records complete maternal and delivery outcomes:
- **Maternal Status**: Survived, Died, or Referred
- **Delivery Information**: Mode of delivery (spontaneous vaginal, assisted, C-section, breech)
- **Perineal Status**: Intact, tears (1st-4th degree), episiotomy
- **Placental Information**: Delivery time, completeness, estimated blood loss
- **Maternal Complications**: PPH, eclampsia, septic shock, obstructed labor, ruptured uterus
- **Post-delivery Care**: Oxytocin, antibiotics, blood transfusion tracking

#### BabyDetails Model (WHO 2020 Compliant)
Complete newborn assessment based on WHO Maternal and Perinatal Health Standards:
- **Identification**: Baby number, tag (for multiples)
- **Vital Status**: Live birth, stillbirth (fresh/macerated), early neonatal death, survived
- **Anthropometric Measurements**: Birth weight, length, head circumference, chest circumference
- **APGAR Scores**: 1, 5, and 10 minutes (with interpretation guidelines)
- **Resuscitation**: Required steps, duration, interventions (oxygen, intubation, chest compressions, medications)
- **Immediate Newborn Care** (WHO 2020 Pages 88-94):
  - Skin-to-skin contact
  - Early breastfeeding initiation (within 1 hour)
  - Delayed cord clamping (1-3 minutes recommended)
  - Vitamin K prophylaxis
  - Eye prophylaxis
  - Hepatitis B vaccination
- **Thermal Care**: First temperature, kangaroo mother care for LBW
- **Birth Classifications**: Automatic weight and gestational age classification
- **Congenital Issues**: Abnormalities and birth injuries tracking
- **Special Care**: NICU admission, feeding method, complications

#### Referral Model
Comprehensive referral system for higher-level facilities:
- **Referral Types**: Maternal, Neonatal, or Both
- **Urgency Levels**: Emergency, Urgent, Semi-urgent, Routine
- **25+ Pre-defined Referral Reasons**:
  - Maternal: Prolonged labor, obstructed labor, fetal distress, hemorrhage, pre-eclampsia/eclampsia, septic shock, ruptured uterus, abnormal presentation, cord prolapse, placenta previa/abruption
  - Neonatal: Asphyxia, prematurity complications, low birth weight, respiratory distress, congenital abnormalities, sepsis, birth injuries
  - Resource: Lack of resources, requires C-section, blood transfusion, specialized care
- **Facility Information**: Referring and destination facility details
- **Clinical Summary**: Maternal and fetal/neonatal condition at referral
- **Labor Status**: Cervical dilation, membrane status, liquor color
- **Interventions**: Medications, IV fluids, investigations performed
- **Transport Details**: Mode, accompanying staff, equipment sent
- **Referral Form Generation**: Auto-populated summary for transfer

### 2. Database Layer

#### Repositories Created
- **BirthOutcomeRepository**: CRUD operations for birth outcomes
- **BabyDetailsRepository**: Supports multiple babies per partograph
- **ReferralRepository**: Referral tracking and management

#### Database Schema Updates
- Added `Tbl_BirthOutcome` with full sync metadata
- Added `Tbl_BabyDetails` with WHO-compliant fields
- Added `Tbl_Referral` with comprehensive referral tracking
- Proper foreign keys to `Tbl_Partograph`
- Indexes for performance and sync operations

### 3. UI Layer (Page Models)

#### BirthOutcomePageModel
- Records delivery outcome for mother and babies
- Validation for maternal death details
- Support for 1-5 babies
- Auto-navigation to baby details entry
- Blood loss validation (0-5000ml)

#### BabyDetailsPageModel
- Multi-baby support with sequential entry
- Baby identification (A, B, C, D, E for multiples)
- WHO 2020 guideline compliance
- APGAR score validation (0-10)
- Automatic birth weight classification
- Resuscitation details tracking
- Navigation between babies

#### ReferralPageModel
- Auto-population from latest partograph readings
- Pre-fills maternal vitals (BP, pulse, temperature)
- Pre-fills labor status (dilation, membrane status, liquor)
- Checklist of 25+ referral reasons
- Referral form generation
- Clinical summary creation

## What's Still Needed

### 1. XAML Pages
You need to create the actual UI pages:

```
MAAME.DROMO.PARTOGRAPH.APP.Droid/Pages/
├── BirthOutcomePage.xaml
├── BirthOutcomePage.xaml.cs
├── BabyDetailsPage.xaml
├── BabyDetailsPage.xaml.cs
├── ReferralPage.xaml
└── ReferralPage.xaml.cs
```

### 2. Route Registration
Register the new pages in `AppShell.xaml.cs` or routing configuration:

```csharp
// In AppShell.xaml.cs or MauiProgram.cs
Routing.RegisterRoute("BirthOutcomePage", typeof(BirthOutcomePage));
Routing.RegisterRoute("BabyDetailsPage", typeof(BabyDetailsPage));
Routing.RegisterRoute("ReferralPage", typeof(ReferralPage));
```

### 3. Dependency Injection
Register repositories and page models in `MauiProgram.cs`:

```csharp
// Repositories
builder.Services.AddSingleton<BirthOutcomeRepository>();
builder.Services.AddSingleton<BabyDetailsRepository>();
builder.Services.AddSingleton<ReferralRepository>();

// Page Models
builder.Services.AddTransient<BirthOutcomePageModel>();
builder.Services.AddTransient<BabyDetailsPageModel>();
builder.Services.AddTransient<ReferralPageModel>();

// Pages
builder.Services.AddTransient<BirthOutcomePage>();
builder.Services.AddTransient<BabyDetailsPage>();
builder.Services.AddTransient<ReferralPage>();
```

### 4. Navigation from Partograph
Add navigation buttons/menu items in your main partograph page:

```csharp
// Navigate to birth outcome recording
await Shell.Current.GoToAsync("BirthOutcomePage", new Dictionary<string, object>
{
    { "PartographId", partograph.ID }
});

// Navigate to referral
await Shell.Current.GoToAsync("ReferralPage", new Dictionary<string, object>
{
    { "PartographId", partograph.ID }
});
```

### 5. PDF Generation (Optional Enhancement)
For production use, implement actual PDF generation for referral forms:
- Use a library like QuestPDF, iTextSharp, or PdfSharpCore
- Generate formatted referral letters
- Include partograph summary charts
- Save to device storage or share via email

### 6. UI Enhancements (Recommended)
- **Step-by-step wizards** for baby details (especially for multiples)
- **Validation indicators** showing which fields are required
- **Auto-save** functionality to prevent data loss
- **Confirmation dialogs** before navigation
- **Progress indicators** for multi-baby entry
- **Referral template selection** for common scenarios

## Usage Flow

### Birth Outcome Recording
1. Complete partograph with delivery
2. Navigate to Birth Outcome page
3. Record maternal status, delivery mode, complications
4. Save outcome
5. Prompted to record baby details

### Baby Details Entry
1. Accessed from Birth Outcome or separately
2. Enter details for Baby A (or single baby)
3. Save and continue to next baby if multiple birth
4. Complete all babies
5. Return to partograph

### Referral Process
1. Identify need for referral during labor/delivery
2. Navigate to Referral page
3. Select referral type and urgency
4. Choose referral reasons from checklist
5. Review auto-populated maternal/fetal condition
6. Enter destination facility details
7. Save and generate referral form
8. Transfer patient with completed referral

## Data Validation

### BirthOutcome
- Maternal death requires time and cause
- Number of babies: 1-5
- Blood loss: 0-5000ml

### BabyDetails
- Birth weight: >0 and ≤10000g
- APGAR scores: 0-10
- Death requires time and cause for non-live births
- Temperature: Reasonable range if specified

### Referral
- Destination facility required
- Primary diagnosis required
- At least one referral reason recommended

## WHO 2020 Compliance

All implementations follow WHO recommendations from:
- **WHO recommendations: intrapartum care for a positive childbirth experience** (2018)
- **WHO recommendations on maternal and perinatal health** (2020)
- **ISBN: 978-92-4-001756-6** (electronic version)

Specific guideline pages referenced in code comments throughout.

## Database Migrations

The new tables will be created automatically on first run through the repository initialization. The schema includes:
- All required fields from models
- Sync metadata for offline capability
- Proper indexes for performance
- Foreign key constraints for data integrity

## Sync Considerations

All new entities support offline sync:
- Sync metadata columns included
- Device tracking for multi-device scenarios
- Conflict detection and resolution support
- Version tracking for optimistic concurrency

## Next Steps

1. **Create XAML Pages**: Design the UI for each page model
2. **Register Routes**: Set up navigation
3. **Configure DI**: Register services
4. **Test Workflow**: End-to-end testing of birth outcome → baby details → referral
5. **Enhance PDF**: Implement professional referral form generation
6. **Add Reporting**: Create summary reports for completed partographs

## Files Added/Modified

### New Model Files
- `MAAME.DROMO.PARTOGRAPH.MODEL/BirthOutcome.cs`
- `MAAME.DROMO.PARTOGRAPH.MODEL/BabyDetails.cs`
- `MAAME.DROMO.PARTOGRAPH.MODEL/Referral.cs`

### New Repository Files
- `MAAME.DROMO.PARTOGRAPH.APP.Droid/Data/BirthOutcomeRepository.cs`
- `MAAME.DROMO.PARTOGRAPH.APP.Droid/Data/BabyDetailsRepository.cs`
- `MAAME.DROMO.PARTOGRAPH.APP.Droid/Data/ReferralRepository.cs`

### New Page Model Files
- `MAAME.DROMO.PARTOGRAPH.APP.Droid/PageModels/BirthOutcomePageModel.cs`
- `MAAME.DROMO.PARTOGRAPH.APP.Droid/PageModels/BabyDetailsPageModel.cs`
- `MAAME.DROMO.PARTOGRAPH.APP.Droid/PageModels/ReferralPageModel.cs`

### Modified Files
- `MAAME.DROMO.PARTOGRAPH.SERVICE/Data/PartographDbContext.cs`

## Commit Information
- **Branch**: `claude/partograph-second-stage-01BgYXiR35MQ4ddvu5Uqi6Y2`
- **Commit**: `997c5fa`
- **Files Changed**: 10 files
- **Lines Added**: 3426+

---

**Implementation Complete!** All backend logic, data models, and page models are ready. The next step is to create the XAML UI pages to complete the user interface.
