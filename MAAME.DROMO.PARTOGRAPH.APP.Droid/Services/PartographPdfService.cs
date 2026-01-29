using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public interface IPartographPdfService
    {
        Task<string> GenerateAndSavePartographPdfAsync(Guid partographId, string patientName);
    }

    public class PartographPdfService : IPartographPdfService
    {
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly FHRRepository _fhrRepository;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly UrineRepository _urineRepository;
        private readonly OxytocinRepository _oxytocinRepository;
        private readonly MedicationEntryRepository _medicationEntryRepository;
        private readonly IVFluidEntryRepository _ivFluidEntryRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly FetalPositionRepository _fetalPositionRepository;
        private readonly AmnioticFluidRepository _amnioticFluidRepository;
        private readonly CaputRepository _caputRepository;
        private readonly MouldingRepository _mouldingRepository;
        private readonly CompanionRepository _companionRepository;
        private readonly PainReliefRepository _painReliefRepository;
        private readonly OralFluidRepository _oralFluidRepository;
        private readonly PostureRepository _postureRepository;
        private readonly AssessmentRepository _assessmentRepository;
        private readonly PlanRepository _planRepository;

        public PartographPdfService(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            FHRRepository fhrRepository,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            ContractionRepository contractionRepository,
            CervixDilatationRepository cervixDilatationRepository,
            UrineRepository urineRepository,
            OxytocinRepository oxytocinRepository,
            MedicationEntryRepository medicationEntryRepository,
            IVFluidEntryRepository ivFluidEntryRepository,
            HeadDescentRepository headDescentRepository,
            FetalPositionRepository fetalPositionRepository,
            AmnioticFluidRepository amnioticFluidRepository,
            CaputRepository caputRepository,
            MouldingRepository mouldingRepository,
            CompanionRepository companionRepository,
            PainReliefRepository painReliefRepository,
            OralFluidRepository oralFluidRepository,
            PostureRepository postureRepository,
            AssessmentRepository assessmentRepository,
            PlanRepository planRepository)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _fhrRepository = fhrRepository;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
            _contractionRepository = contractionRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _urineRepository = urineRepository;
            _oxytocinRepository = oxytocinRepository;
            _medicationEntryRepository = medicationEntryRepository;
            _ivFluidEntryRepository = ivFluidEntryRepository;
            _headDescentRepository = headDescentRepository;
            _fetalPositionRepository = fetalPositionRepository;
            _amnioticFluidRepository = amnioticFluidRepository;
            _caputRepository = caputRepository;
            _mouldingRepository = mouldingRepository;
            _companionRepository = companionRepository;
            _painReliefRepository = painReliefRepository;
            _oralFluidRepository = oralFluidRepository;
            _postureRepository = postureRepository;
            _assessmentRepository = assessmentRepository;
            _planRepository = planRepository;
        }

        public async Task<string> GenerateAndSavePartographPdfAsync(Guid partographId, string patientName)
        {
            try
            {
                // 1. Load Data
                var partograph = await LoadFullPartographAsync(partographId);
                if (partograph == null) throw new Exception("Partograph not found");

                // 2. Create PDF Document
                using PdfDocument doc = new PdfDocument();
                doc.PageSettings.Margins.All = 20;

                // Add Page
                PdfPage page = doc.Pages.Add();
                PdfGraphics graphics = page.Graphics;

                // Fonts
                PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
                PdfFont subHeaderFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
                PdfFont contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                PdfFont smallFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);

                float yPos = 0;

                // 3. Draw Header
                graphics.DrawString("CLINICAL PARTOGRAPH REPORT", headerFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(0, yPos));
                yPos += 25;
                graphics.DrawString($"Date Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", smallFont, PdfBrushes.Gray, new Syncfusion.Drawing.PointF(0, yPos));
                yPos += 20;

                // Patient Info
                graphics.DrawString($"Patient Information", subHeaderFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(0, yPos));
                yPos += 20;
                string patientInfo = $"Name: {patientName}\n" +
                                     $"Hospital No: {partograph.Patient?.HospitalNumber ?? "N/A"}\n" +
                                     $"Admission: {partograph.AdmissionDate:yyyy-MM-dd HH:mm}\n" +
                                     $"Gravida/Parity: G{partograph.Gravida} P{partograph.Parity}";
                
                graphics.DrawString(patientInfo, contentFont, PdfBrushes.Black, new RectangleF(0, yPos, page.GetClientSize().Width, 60));
                yPos += 60;

                // 4. Draw Tables
                yPos = DrawFetalWellbeing(doc, page, partograph, yPos);
                yPos = DrawLaborProgress(doc, page, partograph, yPos);
                yPos = DrawMaternalWellbeing(doc, page, partograph, yPos);
                yPos = DrawInterventions(doc, page, partograph, yPos);

                // 5. Save Logic
                using MemoryStream stream = new MemoryStream();
                doc.Save(stream);
                string fileName = $"Partograph_{patientName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                return await SavePdfToDownloadsAsync(stream.ToArray(), fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate partograph PDF: {ex.Message}", ex);
            }
        }

        private float DrawFetalWellbeing(PdfDocument doc, PdfPage page, Partograph p, float yPos)
        {
            PdfFont sectionFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            page.Graphics.DrawString("Fetal Wellbeing (FHR & Amniotic Fluid)", sectionFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(0, yPos));
            yPos += 20;

            PdfGrid grid = new PdfGrid();
            grid.Style.CellPadding.All = 3;
            grid.Headers.Add(1);
            grid.Headers[0].Cells[0].Value = "Time";
            grid.Headers[0].Cells[1].Value = "FHR (bpm)";
            grid.Headers[0].Cells[2].Value = "Liquor";
            grid.Headers[0].Cells[3].Value = "Moulding";
            grid.Columns.Add(4);

            var times = p.Fhrs.Select(x => x.Time)
                .Union(p.AmnioticFluids.Select(x => x.Time))
                .Union(p.Mouldings.Select(x => x.Time))
                .OrderBy(x => x)
                .ToList();

            if (!times.Any())
            {
                 page.Graphics.DrawString("No data recorded.", new PdfStandardFont(PdfFontFamily.Helvetica, 10), PdfBrushes.Gray, new Syncfusion.Drawing.PointF(10, yPos));
                 return yPos + 20;
            }

            foreach (var time in times)
            {
                PdfGridRow row = grid.Rows.Add();
                row.Cells[0].Value = time.ToString("HH:mm");

                var fhr = p.Fhrs.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                row.Cells[1].Value = fhr?.Rate.ToString() ?? "-";

                var liquor = p.AmnioticFluids.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                // Assuming AmnioticFluid has 'Status'. Checked earlier usage in AlertEngine
                row.Cells[2].Value = liquor?.Color ?? "-";
                
                var moulding = p.Mouldings.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                row.Cells[3].Value = moulding?.Degree ?? "-";
            }

            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            
            PdfGridLayoutResult result = grid.Draw(page, new Syncfusion.Drawing.PointF(0, yPos), layoutFormat);
            return result.Bounds.Bottom + 20;
        }

        private float DrawLaborProgress(PdfDocument doc, PdfPage page, Partograph p, float yPos)
        {
            // Check if we need a new page
            if (yPos > page.GetClientSize().Height - 100)
            {
                page = doc.Pages.Add();
                yPos = 20;
            }

            PdfFont sectionFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            page.Graphics.DrawString("Labour Progress (Dilation, Descent, Contractions)", sectionFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(0, yPos));
            yPos += 20;

            PdfGrid grid = new PdfGrid();
            grid.Style.CellPadding.All = 3;
            grid.Columns.Add(4);
            grid.Headers.Add(1);
            grid.Headers[0].Cells[0].Value = "Time";
            grid.Headers[0].Cells[1].Value = "Cervical Dilation (cm)";
            grid.Headers[0].Cells[2].Value = "Head Descent";
            grid.Headers[0].Cells[3].Value = "Contractions (/10min)";

            var times = p.Dilatations.Select(x => x.Time)
                .Union(p.HeadDescents.Select(x => x.Time))
                .Union(p.Contractions.Select(x => x.Time))
                .OrderBy(x => x)
                .ToList();

            if (!times.Any())
            {
                 page.Graphics.DrawString("No data recorded.", new PdfStandardFont(PdfFontFamily.Helvetica, 10), PdfBrushes.Gray, new Syncfusion.Drawing.PointF(10, yPos));
                 return yPos + 20;
            }

            foreach (var time in times)
            {
                PdfGridRow row = grid.Rows.Add();
                row.Cells[0].Value = time.ToString("HH:mm");

                var dil = p.Dilatations.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                row.Cells[1].Value = dil?.DilatationCm.ToString() ?? "-";

                var descent = p.HeadDescents.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                row.Cells[2].Value = descent?.PalpableAbdominally ?? "-";

                var contr = p.Contractions.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                // Contractions FrequencyPer10Min verified in AlertEngine use.
                row.Cells[3].Value = contr != null ? $"{contr.FrequencyPer10Min} ({contr.DurationSeconds}s)" : "-";
            }

            PdfGridLayoutResult result = grid.Draw(page, new Syncfusion.Drawing.PointF(0, yPos));
            return result.Bounds.Bottom + 20;
        }

        private float DrawMaternalWellbeing(PdfDocument doc, PdfPage page, Partograph p, float yPos)
        {
             if (yPos > page.GetClientSize().Height - 100)
            {
                page = doc.Pages.Add();
                yPos = 20;
            }

            PdfFont sectionFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            page.Graphics.DrawString("Maternal Wellbeing (BP, Pulse, Temp, Urine)", sectionFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(0, yPos));
            yPos += 20;

            PdfGrid grid = new PdfGrid();
            grid.Style.CellPadding.All = 3;
            grid.Columns.Add(5);
            grid.Headers.Add(1);
            grid.Headers[0].Cells[0].Value = "Time";
            grid.Headers[0].Cells[1].Value = "BP (mmHg)";
            grid.Headers[0].Cells[2].Value = "Pulse (bpm)";
            grid.Headers[0].Cells[3].Value = "Temp (°C)";
            grid.Headers[0].Cells[4].Value = "Urine";

            var times = p.BPs.Select(x => x.Time)
                .Union(p.Temperatures.Select(x => x.Time))
                .Union(p.Urines.Select(x => x.Time))
                .OrderBy(x => x)
                .ToList();

            if (!times.Any())
            {
                 page.Graphics.DrawString("No data recorded.", new PdfStandardFont(PdfFontFamily.Helvetica, 10), PdfBrushes.Gray, new Syncfusion.Drawing.PointF(10, yPos));
                 return yPos + 20;
            }

            foreach (var time in times)
            {
                PdfGridRow row = grid.Rows.Add();
                row.Cells[0].Value = time.ToString("HH:mm");

                var bp = p.BPs.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                row.Cells[1].Value = bp != null ? $"{bp.Systolic}/{bp.Diastolic}" : "-";
                // Verified Pulse is part of BP in AlertEngine usage (latestBP.Pulse)
                row.Cells[2].Value = bp?.Pulse.ToString() ?? "-"; 

                var temp = p.Temperatures.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                row.Cells[3].Value = temp?.TemperatureCelsius.ToString() ?? "-";

                var urine = p.Urines.FirstOrDefault(x => Math.Abs((x.Time - time).TotalMinutes) < 5);
                // Verified Urine properties in AlertEngine.cs (Protein, Ketones) and Volume assumed.
                row.Cells[4].Value = urine != null ? $"{urine.TotalIVIntakeMl}ml ({urine.Protein}/{urine.Ketones})" : "-";
            }

            PdfGridLayoutResult result = grid.Draw(page, new Syncfusion.Drawing.PointF(0, yPos));
            return result.Bounds.Bottom + 20;
        }

        private float DrawInterventions(PdfDocument doc, PdfPage page, Partograph p, float yPos)
        {
             if (yPos > page.GetClientSize().Height - 100)
            {
                page = doc.Pages.Add();
                yPos = 20;
            }

            PdfFont sectionFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            page.Graphics.DrawString("Drugs, IV Fluids & Oxytocin", sectionFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(0, yPos));
            yPos += 20;

            PdfGrid grid = new PdfGrid();
            grid.Style.CellPadding.All = 3;
            grid.Columns.Add(3);
            grid.Headers.Add(1);
            grid.Headers[0].Cells[0].Value = "Time";
            grid.Headers[0].Cells[1].Value = "Type";
            grid.Headers[0].Cells[2].Value = "Details";

            // Combine Oxytocin, Meds, IV Fluids
            var oxytocins = p.Oxytocins.Select(x => new { Time = x.Time, Type = "Oxytocin", Details = $"{x.ConcentrationMUnitsPerMl}U, {x.DoseMUnitsPerMin} dpm" });
            // Assuming Medications has Name/Dose
            var meds = p.Medications.Select(x => new { Time = x.Time, Type = "Medication", Details = $"{x.MedicationName} {x.Dose}" });
            // Assuming IVFluids has Type/Volume
            var fluids = p.IVFluids.Select(x => new { Time = x.Time, Type = "IV Fluid", Details = $"{x.FluidType} {x.VolumeInfused}ml" });

            var allInterventions = oxytocins.Concat(meds).Concat(fluids).OrderBy(x => x.Time).ToList();

            if (!allInterventions.Any())
            {
                 page.Graphics.DrawString("No data recorded.", new PdfStandardFont(PdfFontFamily.Helvetica, 10), PdfBrushes.Gray, new Syncfusion.Drawing.PointF(10, yPos));
                 return yPos + 20;
            }

            foreach (var item in allInterventions)
            {
                PdfGridRow row = grid.Rows.Add();
                row.Cells[0].Value = item.Time.ToString("HH:mm");
                row.Cells[1].Value = item.Type;
                row.Cells[2].Value = item.Details;
            }

            PdfGridLayoutResult result = grid.Draw(page, new Syncfusion.Drawing.PointF(0, yPos));
            return result.Bounds.Bottom + 20;
        }

        private async Task<Partograph?> LoadFullPartographAsync(Guid id)
        {
            var partographList = await _partographRepository.ListByPatientAsync(null); 
            var p = partographList.FirstOrDefault(x => x.ID == id);
            
            if (p == null) {
                 var list = await _partographRepository.ListAsync();
                 p = list.FirstOrDefault(x => x.ID == id);
            }

            if (p == null) return null;

            if (p.PatientID.HasValue)
                p.Patient = await _patientRepository.GetAsync(p.PatientID);

            if (p.PatientID == null) return p;
            var patientId = p.PatientID;

            p.Fhrs = await _fhrRepository.ListByPatientAsync(patientId);
            p.BPs = await _bpRepository.ListByPatientAsync(patientId);
            p.Temperatures = await _temperatureRepository.ListByPatientAsync(patientId);
            p.Contractions = await _contractionRepository.ListByPatientAsync(patientId);
            p.Dilatations = await _cervixDilatationRepository.ListByPatientAsync(patientId);
            p.Urines = await _urineRepository.ListByPatientAsync(patientId);
            p.Oxytocins = await _oxytocinRepository.ListByPatientAsync(patientId);
            p.Medications = await _medicationEntryRepository.ListByPatientAsync(patientId);
            p.IVFluids = await _ivFluidEntryRepository.ListByPatientAsync(patientId);
            p.HeadDescents = await _headDescentRepository.ListByPatientAsync(patientId);
            p.FetalPositions = await _fetalPositionRepository.ListByPatientAsync(patientId);
            p.AmnioticFluids = await _amnioticFluidRepository.ListByPatientAsync(patientId);
            p.Caputs = await _caputRepository.ListByPatientAsync(patientId);
            p.Mouldings = await _mouldingRepository.ListByPatientAsync(patientId);
            
            return p;
        }

        private async Task<string> SavePdfToDownloadsAsync(byte[] pdfBytes, string fileName)
        {
#if ANDROID
            var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            
            var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
            mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(filePath)));
            Android.App.Application.Context.SendBroadcast(mediaScanIntent);
            return filePath;
#elif IOS || MACCATALYST
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
#elif WINDOWS
            var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadsPath = Path.Combine(downloadsPath, "Downloads");
            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
#else
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
#endif
        }
    }
}
