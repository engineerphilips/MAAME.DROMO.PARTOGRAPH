using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class SeedDataService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly TagRepository _tagRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly VitalSignRepository _vitalSignRepository;
        private readonly string _seedDataFilePath = "SeedData.json";
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ProjectRepository projectRepository, TaskRepository taskRepository, TagRepository tagRepository, CategoryRepository categoryRepository, PatientRepository patientRepository, PartographRepository partographRepository, VitalSignRepository vitalSignRepository, ILogger<SeedDataService> logger)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _categoryRepository = categoryRepository;
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _vitalSignRepository = vitalSignRepository;
            _logger = logger;
        }

        public async Task LoadSeedDataAsync()
        {
            ClearTables();

            await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(_seedDataFilePath);

            ProjectsJson? payload = null;
            try
            {
                payload = JsonSerializer.Deserialize(templateStream, JsonContext.Default.ProjectsJson);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deserializing seed data");
            }

            try
            {
                if (payload is not null)
                {
                    foreach (var project in payload.Projects)
                    {
                        if (project is null)
                        {
                            continue;
                        }

                        if (project.Category is not null)
                        {
                            await _categoryRepository.SaveItemAsync(project.Category);
                            project.CategoryID = project.Category.ID;
                        }

                        await _projectRepository.SaveItemAsync(project);

                        if (project?.Tasks is not null)
                        {
                            foreach (var task in project.Tasks)
                            {
                                task.ProjectID = project.ID;
                                await _taskRepository.SaveItemAsync(task);
                            }
                        }

                        if (project?.Tags is not null)
                        {
                            foreach (var tag in project.Tags)
                            {
                                await _tagRepository.SaveItemAsync(tag, project.ID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving seed data");
                throw;
            }
        }

        // Add this method to the existing SeedDataService
        public async Task LoadSamplePartographData()
        {
            try
            {
                // Clear existing tables if needed
                // await ClearPartographTables();

                // Create sample patients
                var patients = new List<Patient>
                {
                    new Patient
                    {
                        FirstName = "Mary",
                        LastName = "Johnson",
                        HospitalNumber = "HN2024001",
                        DateOfBirth = new DateTime(1995, 8, 20),
                        Age = 28,
                        //Gravida = 2,
                        //Parity = 1,
                        BloodGroup = "O+",
                        PhoneNumber = "+233501234567",
                        EmergencyContact = "+233509876543",
                        //AdmissionDate = DateTime.Now.AddHours(-8),
                        //ExpectedDeliveryDate = DateTime.Now.AddDays(2),
                        //Status = LaborStatus.Active,
                        //LaborStartTime = DateTime.Now.AddHours(-4),
                        //CervicalDilationOnAdmission = 3,
                        //MembraneStatus = "Intact",
                        //LiquorStatus = "Clear"
                    },
                    new Patient
                    {
                        FirstName = "Grace",
                        LastName = "Mensah",
                        HospitalNumber = "HN2024002",
                        DateOfBirth = new DateTime(1998, 5, 15),
                        Age = 25,
                        //Gravida = 1,
                        //Parity = 0,
                        BloodGroup = "A+",
                        PhoneNumber = "+233502345678",
                        EmergencyContact = "+233508765432",
                        //AdmissionDate = DateTime.Now.AddHours(-12),
                        //ExpectedDeliveryDate = DateTime.Now.AddDays(5),
                        //Status = LaborStatus.Pending,
                        //CervicalDilationOnAdmission = 1,
                        //MembraneStatus = "Intact",
                        //LiquorStatus = "Clear"
                    },
                    new Patient
                    {
                        FirstName = "Akosua",
                        LastName = "Osei",
                        HospitalNumber = "HN2024003",
                        DateOfBirth = new DateTime(1992, 3, 10),
                        Age = 32,
                        //Gravida = 3,
                        //Parity = 2,
                        BloodGroup = "B+",
                        PhoneNumber = "+233503456789",
                        EmergencyContact = "+233507654321",
                        //AdmissionDate = DateTime.Now.AddHours(-20),
                        //ExpectedDeliveryDate = DateTime.Now.AddDays(-1),
                        //Status = LaborStatus.Completed,
                        //LaborStartTime = DateTime.Now.AddHours(-16),
                        //DeliveryTime = DateTime.Now.AddHours(-2),
                        //CervicalDilationOnAdmission = 4,
                        //MembraneStatus = "Ruptured",
                        //LiquorStatus = "Clear"
                    },
                    new Patient
                    {
                        FirstName = "Efua",
                        LastName = "Adu",
                        HospitalNumber = "HN2024004",
                        DateOfBirth = new DateTime(1994, 7, 22),
                        Age = 30,
                        //Gravida = 2,
                        //Parity = 1,
                        BloodGroup = "AB+",
                        PhoneNumber = "+233504567890",
                        EmergencyContact = "+233506543210",
                        //AdmissionDate = DateTime.Now.AddHours(-6),
                        //ExpectedDeliveryDate = DateTime.Now,
                        //Status = LaborStatus.Active,
                        //LaborStartTime = DateTime.Now.AddHours(-3),
                        //CervicalDilationOnAdmission = 5,
                        //MembraneStatus = "Artificial Rupture",
                        //LiquorStatus = "Clear",
                        //RiskFactors = "Previous cesarean section"
                    },
                    new Patient
                    {
                        FirstName = "Abena",
                        LastName = "Kwame",
                        HospitalNumber = "HN2024005",
                        DateOfBirth = new DateTime(1989, 4, 15),
                        Age = 35,
                        //Gravida = 4,
                        //Parity = 3,
                        BloodGroup = "O-",
                        PhoneNumber = "+233505678901",
                        EmergencyContact = "+233505432109",
                        //AdmissionDate = DateTime.Now.AddHours(-2),
                        //ExpectedDeliveryDate = DateTime.Now.AddDays(1),
                        //Status = LaborStatus.Emergency,
                        //LaborStartTime = DateTime.Now.AddHours(-1),
                        //CervicalDilationOnAdmission = 6,
                        //MembraneStatus = "Ruptured",
                        //LiquorStatus = "Meconium Grade II",
                        //RiskFactors = "Advanced maternal age, gestational hypertension",
                        //Complications = "Fetal distress"
                    }
                };

                // Save patients
                foreach (var patient in patients)
                {
                    await _patientRepository.SaveItemAsync(patient);

                    // Add sample partograph entries for active patients
                    //if (patient.Status == LaborStatus.Active && patient.LaborStartTime.HasValue)
                    //{
                    //    await CreateSamplePartographEntries(patient);
                    //}

                    // Add sample vital signs
                    await CreateSampleVitalSigns(patient);
                }

                _logger.LogInformation("Sample partograph data loaded successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading sample partograph data");
                throw;
            }
        }

        private async Task CreateSamplePartographEntries(Patient patient)
        {
            //if (!patient.LaborStartTime.HasValue) return;

            //var startTime = patient.LaborStartTime.Value;
            //var currentDilation = patient.CervicalDilationOnAdmission ?? 3;

            // Create entries every 30 minutes
            for (int i = 0; i < 4; i++)
            {
                var entry = new Partograph
                {
                    PatientID = patient.ID,
                    //Time = startTime.AddMinutes(i * 30),
                    //CervicalDilation = Math.Min(currentDilation + i, 10),
                    //DescentOfHead = i > 2 ? "+1" : "0",
                    //ContractionsPerTenMinutes = Math.Min(2 + i, 4),
                    //ContractionDuration = 30 + (i * 5),
                    //ContractionStrength = i > 2 ? "Strong" : "Moderate",
                    //FetalHeartRate = 140 + Random.Shared.Next(-10, 10),
                    //LiquorStatus = patient.LiquorStatus,
                    //Moulding = i > 2 ? "+" : "None",
                    //Caput = "None",
                    //RecordedBy = "Midwife Sarah",
                    //Notes = i == 0 ? "Labor progressing well" : ""
                };

                await _partographRepository.SaveItemAsync(entry);
            }
        }

        private async Task CreateSampleVitalSigns(Patient patient)
        {
            var vitalSign = new VitalSign
            {
                PatientID = patient.ID,
                //RecordedTime = patient.AdmissionDate.AddMinutes(30),
                SystolicBP = 120 + Random.Shared.Next(-10, 20),
                DiastolicBP = 80 + Random.Shared.Next(-5, 10),
                Temperature = 36.5m + (decimal)(Random.Shared.NextDouble() * 0.8),
                PulseRate = 80 + Random.Shared.Next(-10, 15),
                RespiratoryRate = 16 + Random.Shared.Next(-2, 4),
                UrineOutput = "150ml",
                //UrineProtein = patient.Status == LaborStatus.Emergency ? "+" : "Nil",
                UrineAcetone = "Nil",
                RecordedBy = "Nurse Emma"
            };

            await _vitalSignRepository.SaveItemAsync(vitalSign);
        }

        private async void ClearTables()
        {
            try
            {
                await Task.WhenAll(
                    _projectRepository.DropTableAsync(),
                    _taskRepository.DropTableAsync(),
                    _tagRepository.DropTableAsync(),
                    _categoryRepository.DropTableAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}