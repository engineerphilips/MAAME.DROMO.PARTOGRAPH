using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services
{
    public interface IPartographPdfService
    {
        Task<byte[]> GeneratePartographPdfAsync(Guid partographId);
    }

    public class PartographPdfService : IPartographPdfService
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<PartographPdfService> _logger;

        // Constants for WHO 2020 Partograph layout
        private const float PageWidth = 842f; // A4 landscape
        private const float PageHeight = 595f;
        private const float MarginLeft = 40f;
        private const float MarginTop = 20f;
        private const float AlertColumnWidth = 80f;
        private const float RowHeight = 18f;
        private const float TimeColumnWidth = 45f;

        public PartographPdfService(PartographDbContext context, ILogger<PartographPdfService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> GeneratePartographPdfAsync(Guid partographId)
        {
            try
            {
                // Fetch partograph with all related data
                var partograph = await _context.Partographs
                    .Include(p => p.Patient)
                    .FirstOrDefaultAsync(p => p.ID == partographId && p.Deleted == 0);

                if (partograph == null)
                {
                    throw new Exception($"Partograph {partographId} not found");
                }

                // Fetch all measurements
                var fhrs = await _context.FHRs.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var contractions = await _context.Contractions.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var cervixDilatations = await _context.CervixDilatations.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var headDescents = await _context.HeadDescents.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var bps = await _context.BPs.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var temperatures = await _context.Temperatures.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var urines = await _context.Urines.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var caputs = await _context.Caputs.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var mouldings = await _context.Mouldings.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var fetalPositions = await _context.FetalPositions.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var painReliefs = await _context.PainReliefs.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var postures = await _context.Postures.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var oralFluids = await _context.OralFluids.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var ivFluids = await _context.IVFluids.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var medications = await _context.Medications.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var oxytocins = await _context.Oxytocins.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var companions = await _context.Companions.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var assessments = await _context.Assessments.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var plans = await _context.Plans.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();
                var amnioticFluids = await _context.AmnioticFluids.Where(m => m.PartographID == partographId && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync();

                // Create PDF document (A4 Landscape)
                PdfDocument document = new PdfDocument();
                PdfPage page = document.Pages.Add();
                page.SetSize(PageWidth, PageHeight);
                PdfGraphics graphics = page.Graphics;

                // Fonts
                PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold);
                PdfFont regularFont = new PdfStandardFont(PdfFontFamily.Helvetica, 7);
                PdfFont smallFont = new PdfStandardFont(PdfFontFamily.Helvetica, 6);
                PdfFont titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);

                // Colors
                PdfColor alertColor = new PdfColor(255, 230, 230); // Light red
                PdfColor headerColor = new PdfColor(200, 230, 255); // Light blue
                PdfColor borderColor = new PdfColor(0, 0, 0); // Black
                PdfPen borderPen = new PdfPen(borderColor, 0.5f);

                float yPos = MarginTop;

                // Title
                graphics.DrawString("WHO 2020 Partograph", titleFont, PdfBrushes.Black, new PointF(MarginLeft, yPos));
                yPos += 20;

                // Patient Header Information
                yPos = DrawPatientHeader(graphics, partograph, headerFont, regularFont, yPos);
                yPos += 10;

                // Draw Partograph Grid
                yPos = DrawPartographGrid(graphics, partograph, fhrs, contractions, cervixDilatations, headDescents,
                    bps, temperatures, urines, caputs, mouldings, fetalPositions, painReliefs, postures,
                    oralFluids, ivFluids, medications, oxytocins, companions, assessments, plans, amnioticFluids,
                    regularFont, smallFont, borderPen, alertColor, yPos);

                // Save to memory stream
                using (MemoryStream stream = new MemoryStream())
                {
                    document.Save(stream);
                    document.Close(true);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating partograph PDF for {PartographId}", partographId);
                throw;
            }
        }

        private float DrawPatientHeader(PdfGraphics graphics, Partograph partograph, PdfFont headerFont, PdfFont regularFont, float yPos)
        {
            float xPos = MarginLeft;
            float lineHeight = 15f;

            // Row 1
            graphics.DrawString($"Name: {partograph.Patient?.Name ?? "N/A"}", regularFont, PdfBrushes.Black, new PointF(xPos, yPos));
            graphics.DrawString($"Parity: {partograph.Parity}", regularFont, PdfBrushes.Black, new PointF(xPos + 250, yPos));
            graphics.DrawString($"Labour onset: {partograph.LaborStartTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}", regularFont, PdfBrushes.Black, new PointF(xPos + 400, yPos));
            yPos += lineHeight;

            // Row 2
            graphics.DrawString($"Hospital No: {partograph.Patient?.HospitalNumber ?? "N/A"}", regularFont, PdfBrushes.Black, new PointF(xPos, yPos));
            graphics.DrawString($"Gravida: {partograph.Gravida}", regularFont, PdfBrushes.Black, new PointF(xPos + 250, yPos));
            graphics.DrawString($"Active labour diagnosis (Date/Time): {partograph.FirstStageStartTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}", regularFont, PdfBrushes.Black, new PointF(xPos + 400, yPos));
            yPos += lineHeight;

            // Row 3
            graphics.DrawString($"Date of Admission: {partograph.AdmissionDate:dd/MM/yyyy}", regularFont, PdfBrushes.Black, new PointF(xPos, yPos));
            graphics.DrawString($"Risk factors: {string.Join(", ", partograph.RiskFactors?.Select(r => r.Factor) ?? Array.Empty<string>())}", regularFont, PdfBrushes.Black, new PointF(xPos + 250, yPos));
            yPos += lineHeight;

            // Row 4
            graphics.DrawString($"Ruptured membranes (Date/Time): {(partograph.RupturedMembraneTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A")}", regularFont, PdfBrushes.Black, new PointF(xPos, yPos));
            yPos += lineHeight;

            return yPos;
        }

        private float DrawPartographGrid(PdfGraphics graphics, Partograph partograph,
            List<FHR> fhrs, List<Contraction> contractions, List<CervixDilatation> cervixDilatations,
            List<HeadDescent> headDescents, List<BP> bps, List<Temperature> temperatures,
            List<Urine> urines, List<Caput> caputs, List<Moulding> mouldings,
            List<FetalPosition> fetalPositions, List<PainReliefEntry> painReliefs,
            List<PostureEntry> postures, List<OralFluidEntry> oralFluids,
            List<IVFluidEntry> ivFluids, List<MedicationEntry> medications,
            List<Oxytocin> oxytocins, List<CompanionEntry> companions,
            List<Assessment> assessments, List<Plan> plans, List<AmnioticFluid> amnioticFluids,
            PdfFont regularFont, PdfFont smallFont, PdfPen borderPen, PdfColor alertColor, float startY)
        {
            float xPos = MarginLeft;
            float yPos = startY;

            // Calculate time columns (12 hours for first stage + second stage)
            int totalHours = 16; // Extended to accommodate second stage
            float gridWidth = PageWidth - MarginLeft - 40;
            float dataGridWidth = gridWidth - AlertColumnWidth;
            float columnWidth = dataGridWidth / totalHours;

            // Draw header row with time
            graphics.DrawRectangle(borderPen, new PdfSolidBrush(alertColor), xPos, yPos, AlertColumnWidth, RowHeight);
            graphics.DrawString("Alert", smallFont, PdfBrushes.Black, new RectangleF(xPos + 5, yPos + 5, AlertColumnWidth - 10, RowHeight));
            graphics.DrawString("column", smallFont, PdfBrushes.Black, new RectangleF(xPos + 5, yPos + 11, AlertColumnWidth - 10, RowHeight));

            // Time row
            float timeX = xPos + AlertColumnWidth;
            for (int i = 0; i < 12; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, RowHeight);
                graphics.DrawString((i + 1).ToString(), smallFont, PdfBrushes.Black, new PointF(timeX + columnWidth / 2 - 3, yPos + 5));
                timeX += columnWidth;
            }

            // Add stage labels
            float activeStageWidth = columnWidth * 12;
            graphics.DrawString("ACTIVE FIRST STAGE", smallFont, PdfBrushes.Black, new RectangleF(xPos + AlertColumnWidth, yPos - 10, activeStageWidth, 10));
            graphics.DrawString("SECOND STAGE", smallFont, PdfBrushes.Black, new RectangleF(xPos + AlertColumnWidth + activeStageWidth, yPos - 10, columnWidth * 4, 10));

            yPos += RowHeight;

            // ALERT row
            graphics.DrawRectangle(borderPen, new PdfSolidBrush(alertColor), xPos, yPos, AlertColumnWidth, RowHeight);
            graphics.DrawString("ALERT", regularFont, PdfBrushes.Black, new PointF(xPos + 5, yPos + 5));
            timeX = xPos + AlertColumnWidth;
            for (int i = 0; i < totalHours; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, RowHeight);
                timeX += columnWidth;
            }
            yPos += RowHeight;

            // SUPPORTIVE CARE Section
            yPos = DrawSectionHeader(graphics, "SUPPORTIVE CARE", xPos, yPos, AlertColumnWidth, borderPen, alertColor, regularFont, RowHeight);
            yPos = DrawDataRow(graphics, "Companion", "N", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, companions, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Pain relief", "N", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, painReliefs, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Oral fluid", "N", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, oralFluids, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Posture", "SP", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, postures, partograph, RowHeight);

            // BABY Section
            yPos = DrawSectionHeader(graphics, "BABY", xPos, yPos, AlertColumnWidth, borderPen, alertColor, regularFont, RowHeight);
            yPos = DrawFHRRow(graphics, xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, fhrs, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "FHR deceleration", "<110, >160", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, null, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Amniotic fluid", "M+++, B", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, amnioticFluids, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Fetal position", "P,T", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, fetalPositions, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Caput", "+++", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, caputs, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Moulding", "+++", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, mouldings, partograph, RowHeight);

            // Check if we need a new page
            if (yPos > PageHeight - 100)
            {
                // For now, we'll continue on the same page - in production, add new page logic here
            }

            // WOMAN Section
            yPos = DrawSectionHeader(graphics, "WOMAN", xPos, yPos, AlertColumnWidth, borderPen, alertColor, regularFont, RowHeight);
            yPos = DrawDataRow(graphics, "Pulse", "<60, >120", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, bps, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Systolic BP", "<80, >140", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, bps, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Diastolic BP", ">90", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, bps, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Temperature °C", "<35.0, >37.5", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, temperatures, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Urine", "P++, A++", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, urines, partograph, RowHeight);

            // LABOUR PROGRESS Section
            yPos = DrawSectionHeader(graphics, "LABOUR PROGRESS", xPos, yPos, AlertColumnWidth, borderPen, alertColor, regularFont, RowHeight);
            yPos = DrawDataRow(graphics, "Contractions per 10 min", "≤2, >5", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, contractions, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Duration of contractions", "≥20, >60", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, contractions, partograph, RowHeight);

            // Cervix dilatation plot (larger row with grid)
            yPos = DrawCervixPlotRow(graphics, xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, cervixDilatations, partograph, RowHeight * 6);

            // Descent plot
            yPos = DrawDescentPlotRow(graphics, xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, headDescents, partograph, RowHeight * 3);

            // MEDICATION Section
            yPos = DrawSectionHeader(graphics, "MEDICATION", xPos, yPos, AlertColumnWidth, borderPen, alertColor, regularFont, RowHeight);
            yPos = DrawDataRow(graphics, "Oxytocin (U/L, drops/min)", "", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, oxytocins, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "Medicine", "", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, medications, partograph, RowHeight);
            yPos = DrawDataRow(graphics, "IV fluids", "", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, ivFluids, partograph, RowHeight);

            // SHARED DECISION-MAKING Section
            yPos = DrawSectionHeader(graphics, "SHARED DECISION-MAKING", xPos, yPos, AlertColumnWidth, borderPen, alertColor, regularFont, RowHeight);
            yPos = DrawDataRow(graphics, "ASSESSMENT", "", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, assessments, partograph, RowHeight * 2);
            yPos = DrawDataRow(graphics, "PLAN", "", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, plans, partograph, RowHeight * 2);

            // INITIALS row
            yPos = DrawDataRow(graphics, "INITIALS", "", xPos, yPos, AlertColumnWidth, columnWidth, totalHours, borderPen, smallFont, null, partograph, RowHeight);

            return yPos;
        }

        private float DrawSectionHeader(PdfGraphics graphics, string title, float xPos, float yPos, float alertWidth, PdfPen borderPen, PdfColor alertColor, PdfFont font, float height)
        {
            graphics.DrawRectangle(borderPen, new PdfSolidBrush(alertColor), xPos, yPos, alertWidth, height);
            graphics.DrawString(title, font, PdfBrushes.Black, new RectangleF(xPos + 5, yPos + 5, alertWidth - 10, height), new PdfStringFormat { LineAlignment = PdfVerticalAlignment.Middle });

            // Draw empty cells for data columns
            float timeX = xPos + alertWidth;
            int totalHours = 16;
            float gridWidth = PageWidth - MarginLeft - 40;
            float dataGridWidth = gridWidth - alertWidth;
            float columnWidth = dataGridWidth / totalHours;

            for (int i = 0; i < totalHours; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, height);
                timeX += columnWidth;
            }

            return yPos + height;
        }

        private float DrawDataRow<T>(PdfGraphics graphics, string label, string alertText, float xPos, float yPos,
            float alertWidth, float columnWidth, int totalHours, PdfPen borderPen, PdfFont font,
            List<T> data, Partograph partograph, float height) where T : BasePartographMeasurement
        {
            // Draw alert column
            graphics.DrawRectangle(borderPen, xPos, yPos, alertWidth, height);
            graphics.DrawString(label, font, PdfBrushes.Black, new RectangleF(xPos + 2, yPos + 2, alertWidth - 4, height / 2));
            if (!string.IsNullOrEmpty(alertText))
            {
                graphics.DrawString(alertText, font, PdfBrushes.Black, new RectangleF(xPos + 2, yPos + height / 2, alertWidth - 4, height / 2));
            }

            // Draw data cells
            float timeX = xPos + alertWidth;
            for (int i = 0; i < totalHours; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, height);

                // Plot data points if available
                if (data != null && partograph.LaborStartTime.HasValue)
                {
                    var hourStart = partograph.LaborStartTime.Value.AddHours(i);
                    var hourEnd = hourStart.AddHours(1);

                    var dataInHour = data.Where(d => d.Time >= hourStart && d.Time < hourEnd).ToList();
                    if (dataInHour.Any())
                    {
                        // Draw marker for data point
                        string marker = GetDataMarker(dataInHour.First(), label);
                        if (!string.IsNullOrEmpty(marker))
                        {
                            graphics.DrawString(marker, font, PdfBrushes.Black, new PointF(timeX + columnWidth / 2 - 2, yPos + height / 2 - 3));
                        }
                    }
                }

                timeX += columnWidth;
            }

            return yPos + height;
        }

        private float DrawFHRRow(PdfGraphics graphics, float xPos, float yPos, float alertWidth, float columnWidth,
            int totalHours, PdfPen borderPen, PdfFont font, List<FHR> fhrs, Partograph partograph, float height)
        {
            // Draw alert column
            graphics.DrawRectangle(borderPen, xPos, yPos, alertWidth, height);
            graphics.DrawString("FHR", font, PdfBrushes.Black, new RectangleF(xPos + 2, yPos + 2, alertWidth - 4, height / 2));
            graphics.DrawString("<110, >160", font, PdfBrushes.Black, new RectangleF(xPos + 2, yPos + height / 2, alertWidth - 4, height / 2));

            // Draw data cells with FHR values
            float timeX = xPos + alertWidth;
            for (int i = 0; i < totalHours; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, height);

                if (fhrs != null && partograph.LaborStartTime.HasValue)
                {
                    var hourStart = partograph.LaborStartTime.Value.AddHours(i);
                    var hourEnd = hourStart.AddHours(1);

                    var fhrInHour = fhrs.Where(f => f.Time >= hourStart && f.Time < hourEnd).ToList();
                    if (fhrInHour.Any())
                    {
                        var fhrValue = fhrInHour.First().Rate;
                        graphics.DrawString(fhrValue.ToString(), font, PdfBrushes.Black, new PointF(timeX + columnWidth / 2 - 5, yPos + height / 2 - 3));
                    }
                }

                timeX += columnWidth;
            }

            return yPos + height;
        }

        private float DrawCervixPlotRow(PdfGraphics graphics, float xPos, float yPos, float alertWidth, float columnWidth,
            int totalHours, PdfPen borderPen, PdfFont font, List<CervixDilatation> cervixData, Partograph partograph, float height)
        {
            // Draw alert column
            graphics.DrawRectangle(borderPen, xPos, yPos, alertWidth, height);
            graphics.DrawString("Cervix (Plot X)", font, PdfBrushes.Black, new RectangleF(xPos + 2, yPos + 2, alertWidth - 4, 15));

            // Draw cm markers (10 to 0)
            float yStep = height / 10;
            for (int i = 10; i >= 0; i--)
            {
                float markerY = yPos + (10 - i) * yStep;
                graphics.DrawString(i.ToString(), font, PdfBrushes.Black, new PointF(xPos + alertWidth - 15, markerY - 3));
            }

            // Draw grid and plot cervical dilatation
            float timeX = xPos + alertWidth;
            PointF? prevPoint = null;

            for (int i = 0; i < totalHours; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, height);

                // Draw horizontal grid lines for each cm
                for (int cm = 1; cm < 10; cm++)
                {
                    float gridY = yPos + cm * yStep;
                    PdfPen gridPen = new PdfPen(new PdfColor(200, 200, 200), 0.25f);
                    graphics.DrawLine(gridPen, timeX, gridY, timeX + columnWidth, gridY);
                }

                // Plot cervical dilatation points
                if (cervixData != null && partograph.LaborStartTime.HasValue)
                {
                    var hourStart = partograph.LaborStartTime.Value.AddHours(i);
                    var hourEnd = hourStart.AddHours(1);

                    var cervixInHour = cervixData.Where(c => c.Time >= hourStart && c.Time < hourEnd).FirstOrDefault();
                    if (cervixInHour != null)
                    {
                        float plotY = yPos + (10 - cervixInHour.DilatationCm) * yStep;
                        float plotX = timeX + columnWidth / 2;
                        PointF currentPoint = new PointF(plotX, plotY);

                        // Draw X marker
                        graphics.DrawString("X", font, PdfBrushes.Blue, new PointF(plotX - 3, plotY - 3));

                        // Draw line to previous point
                        if (prevPoint.HasValue)
                        {
                            PdfPen plotPen = new PdfPen(PdfBrushes.Blue, 1f);
                            graphics.DrawLine(plotPen, prevPoint.Value, currentPoint);
                        }

                        prevPoint = currentPoint;
                    }
                }

                timeX += columnWidth;
            }

            return yPos + height;
        }

        private float DrawDescentPlotRow(PdfGraphics graphics, float xPos, float yPos, float alertWidth, float columnWidth,
            int totalHours, PdfPen borderPen, PdfFont font, List<HeadDescent> descentData, Partograph partograph, float height)
        {
            // Draw alert column
            graphics.DrawRectangle(borderPen, xPos, yPos, alertWidth, height);
            graphics.DrawString("Descent (Plot O)", font, PdfBrushes.Black, new RectangleF(xPos + 2, yPos + 2, alertWidth - 4, 15));

            // Draw station markers (5 to 0)
            float yStep = height / 5;
            for (int i = 5; i >= 0; i--)
            {
                float markerY = yPos + (5 - i) * yStep;
                graphics.DrawString(i.ToString(), font, PdfBrushes.Black, new PointF(xPos + alertWidth - 15, markerY - 3));
            }

            // Draw grid and plot head descent
            float timeX = xPos + alertWidth;
            PointF? prevPoint = null;

            for (int i = 0; i < totalHours; i++)
            {
                graphics.DrawRectangle(borderPen, timeX, yPos, columnWidth, height);

                // Plot descent points
                if (descentData != null && partograph.LaborStartTime.HasValue)
                {
                    var hourStart = partograph.LaborStartTime.Value.AddHours(i);
                    var hourEnd = hourStart.AddHours(1);

                    var descentInHour = descentData.Where(d => d.Time >= hourStart && d.Time < hourEnd).FirstOrDefault();
                    if (descentInHour != null)
                    {
                        // Map station (-3 to +3) to display (5 to 0)
                        int displayStation = Math.Max(0, Math.Min(5, 5 - (descentInHour.Station + 3)));
                        float plotY = yPos + displayStation * yStep;
                        float plotX = timeX + columnWidth / 2;
                        PointF currentPoint = new PointF(plotX, plotY);

                        // Draw O marker
                        graphics.DrawString("O", font, PdfBrushes.Red, new PointF(plotX - 3, plotY - 3));

                        // Draw line to previous point
                        if (prevPoint.HasValue)
                        {
                            PdfPen plotPen = new PdfPen(PdfBrushes.Red, 1f);
                            graphics.DrawLine(plotPen, prevPoint.Value, currentPoint);
                        }

                        prevPoint = currentPoint;
                    }
                }

                timeX += columnWidth;
            }

            return yPos + height;
        }

        private string GetDataMarker<T>(T data, string label) where T : BasePartographMeasurement
        {
            return label switch
            {
                "Companion" => data is CompanionEntry c ? (c.Present ? "Y" : "N") : "",
                "Pain relief" => data is PainReliefEntry pr ? (pr.Given ? "Y" : "N") : "",
                "Oral fluid" => data is OralFluidEntry of ? (of.Given ? "Y" : "N") : "",
                "Posture" => data is PostureEntry p ? (p.Upright ? "U" : "S") : "",
                "Amniotic fluid" => data is AmnioticFluid af ? af.Status : "",
                "Fetal position" => data is FetalPosition fp ? fp.Position : "",
                "Caput" => data is Caput cap ? new string('+', cap.Grade) : "",
                "Moulding" => data is Moulding m ? new string('+', m.Grade) : "",
                "Pulse" => data is BP bp ? bp.Pulse.ToString() : "",
                "Systolic BP" => data is BP bps ? bps.Systolic.ToString() : "",
                "Diastolic BP" => data is BP bpd ? bpd.Diastolic.ToString() : "",
                "Temperature °C" => data is Temperature t ? t.Value.ToString("F1") : "",
                "Urine" => data is Urine u ? (u.Protein > 0 ? $"P{new string('+', u.Protein)}" : "") + (u.Acetone > 0 ? $"A{new string('+', u.Acetone)}" : "") : "",
                "Contractions per 10 min" => data is Contraction con ? con.Frequency.ToString() : "",
                "Duration of contractions" => data is Contraction cond ? cond.Duration.ToString() : "",
                "Oxytocin (U/L, drops/min)" => data is Oxytocin ox ? $"{ox.Concentration}U/{ox.DropsPerMin}" : "",
                "Medicine" => data is MedicationEntry med ? med.Name : "",
                "IV fluids" => data is IVFluidEntry iv ? iv.FluidType : "",
                "ASSESSMENT" => data is Assessment a ? a.Note : "",
                "PLAN" => data is Plan pl ? pl.Note : "",
                _ => ""
            };
        }
    }
}
