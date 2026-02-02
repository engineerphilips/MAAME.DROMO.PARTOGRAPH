using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Data
{
    public class DataSeeder
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(PartographDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAllDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding...");

                await SeedRegionsAsync();
                await SeedDistrictsAsync();
                await UpdateFacilitiesWithRegionDistrictAsync();
                await SeedMonitoringUsersAsync();

                _logger.LogInformation("Data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data seeding");
                throw;
            }
        }

        #region Regions

        private async Task SeedRegionsAsync()
        {
            if (await _context.Regions.AnyAsync())
            {
                _logger.LogInformation("Regions already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding regions...");

            var regions = GetGhanaRegions();
            await _context.Regions.AddRangeAsync(regions);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {regions.Count} regions.");
        }

        private List<Region> GetGhanaRegions()
        {
            var now = DateTime.UtcNow;
            return new List<Region>
            {
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                    Name = "Greater Accra",
                    Code = "GAR",
                    Country = "Ghana",
                    Capital = "Accra",
                    Population = 5455692,
                    ExpectedAnnualDeliveries = 163671,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-302-000-001",
                    Email = "gar@ghs.gov.gh",
                    Latitude = 5.6037,
                    Longitude = -0.1870,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                    Name = "Ashanti",
                    Code = "ASH",
                    Country = "Ghana",
                    Capital = "Kumasi",
                    Population = 5792187,
                    ExpectedAnnualDeliveries = 173766,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-322-000-001",
                    Email = "ash@ghs.gov.gh",
                    Latitude = 6.6885,
                    Longitude = -1.6244,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                    Name = "Western",
                    Code = "WR",
                    Country = "Ghana",
                    Capital = "Sekondi-Takoradi",
                    Population = 2060585,
                    ExpectedAnnualDeliveries = 61818,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-312-000-001",
                    Email = "wr@ghs.gov.gh",
                    Latitude = 5.0107,
                    Longitude = -1.9535,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                    Name = "Central",
                    Code = "CR",
                    Country = "Ghana",
                    Capital = "Cape Coast",
                    Population = 2563228,
                    ExpectedAnnualDeliveries = 76897,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-332-000-001",
                    Email = "cr@ghs.gov.gh",
                    Latitude = 5.1315,
                    Longitude = -1.2795,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                    Name = "Eastern",
                    Code = "ER",
                    Country = "Ghana",
                    Capital = "Koforidua",
                    Population = 3037913,
                    ExpectedAnnualDeliveries = 91137,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-342-000-001",
                    Email = "er@ghs.gov.gh",
                    Latitude = 6.0940,
                    Longitude = -0.2577,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111106"),
                    Name = "Volta",
                    Code = "VR",
                    Country = "Ghana",
                    Capital = "Ho",
                    Population = 1651632,
                    ExpectedAnnualDeliveries = 49549,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-362-000-001",
                    Email = "vr@ghs.gov.gh",
                    Latitude = 6.6000,
                    Longitude = 0.4700,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111107"),
                    Name = "Oti",
                    Code = "OTI",
                    Country = "Ghana",
                    Capital = "Dambai",
                    Population = 759799,
                    ExpectedAnnualDeliveries = 22794,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-363-000-001",
                    Email = "oti@ghs.gov.gh",
                    Latitude = 7.8500,
                    Longitude = 0.1800,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111108"),
                    Name = "Northern",
                    Code = "NR",
                    Country = "Ghana",
                    Capital = "Tamale",
                    Population = 2479461,
                    ExpectedAnnualDeliveries = 74384,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-372-000-001",
                    Email = "nr@ghs.gov.gh",
                    Latitude = 9.4008,
                    Longitude = -0.8393,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111109"),
                    Name = "Savannah",
                    Code = "SVR",
                    Country = "Ghana",
                    Capital = "Damongo",
                    Population = 649627,
                    ExpectedAnnualDeliveries = 19489,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-373-000-001",
                    Email = "svr@ghs.gov.gh",
                    Latitude = 9.0833,
                    Longitude = -1.8167,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111110"),
                    Name = "North East",
                    Code = "NER",
                    Country = "Ghana",
                    Capital = "Nalerigu",
                    Population = 648854,
                    ExpectedAnnualDeliveries = 19466,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-374-000-001",
                    Email = "ner@ghs.gov.gh",
                    Latitude = 10.5167,
                    Longitude = -0.3667,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Upper East",
                    Code = "UER",
                    Country = "Ghana",
                    Capital = "Bolgatanga",
                    Population = 1301226,
                    ExpectedAnnualDeliveries = 39037,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-382-000-001",
                    Email = "uer@ghs.gov.gh",
                    Latitude = 10.7856,
                    Longitude = -0.8519,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                    Name = "Upper West",
                    Code = "UWR",
                    Country = "Ghana",
                    Capital = "Wa",
                    Population = 901502,
                    ExpectedAnnualDeliveries = 27045,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-392-000-001",
                    Email = "uwr@ghs.gov.gh",
                    Latitude = 10.0601,
                    Longitude = -2.5099,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111113"),
                    Name = "Bono",
                    Code = "BR",
                    Country = "Ghana",
                    Capital = "Sunyani",
                    Population = 1208649,
                    ExpectedAnnualDeliveries = 36259,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-352-000-001",
                    Email = "br@ghs.gov.gh",
                    Latitude = 7.3349,
                    Longitude = -2.3123,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111114"),
                    Name = "Bono East",
                    Code = "BER",
                    Country = "Ghana",
                    Capital = "Techiman",
                    Population = 1174521,
                    ExpectedAnnualDeliveries = 35236,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-353-000-001",
                    Email = "ber@ghs.gov.gh",
                    Latitude = 7.5833,
                    Longitude = -1.9333,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111115"),
                    Name = "Ahafo",
                    Code = "AHR",
                    Country = "Ghana",
                    Capital = "Goaso",
                    Population = 564536,
                    ExpectedAnnualDeliveries = 16936,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-354-000-001",
                    Email = "ahr@ghs.gov.gh",
                    Latitude = 6.8000,
                    Longitude = -2.5167,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                },
                new Region
                {
                    ID = Guid.Parse("11111111-1111-1111-1111-111111111116"),
                    Name = "Western North",
                    Code = "WNR",
                    Country = "Ghana",
                    Capital = "Sefwi Wiawso",
                    Population = 910366,
                    ExpectedAnnualDeliveries = 27311,
                    DirectorName = "Dr. Regional Director",
                    Phone = "+233-313-000-001",
                    Email = "wnr@ghs.gov.gh",
                    Latitude = 6.2000,
                    Longitude = -2.4833,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                }
            };
        }

        #endregion

        #region Districts

        private async Task SeedDistrictsAsync()
        {
            if (await _context.Districts.AnyAsync())
            {
                _logger.LogInformation("Districts already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding districts...");

            var districts = GetGhanaDistricts();
            await _context.Districts.AddRangeAsync(districts);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {districts.Count} districts.");
        }

        private List<District> GetGhanaDistricts()
        {
            var now = DateTime.UtcNow;
            var districts = new List<District>();

            // Greater Accra Region Districts
            var greaterAccraId = Guid.Parse("11111111-1111-1111-1111-111111111101");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220001", "Accra Metropolitan", "AMA", "Metropolitan", greaterAccraId, "Greater Accra", "Accra", 1665086, 49953),
                CreateDistrict("22222222-2222-2222-2222-222222220002", "Tema Metropolitan", "TMA", "Metropolitan", greaterAccraId, "Greater Accra", "Tema", 292773, 8783),
                CreateDistrict("22222222-2222-2222-2222-222222220003", "Ga East Municipal", "GEMA", "Municipal", greaterAccraId, "Greater Accra", "Abokobi", 259668, 7790),
                CreateDistrict("22222222-2222-2222-2222-222222220004", "Ga West Municipal", "GWMA", "Municipal", greaterAccraId, "Greater Accra", "Amasaman", 262742, 7882),
                CreateDistrict("22222222-2222-2222-2222-222222220005", "Ga South Municipal", "GSMA", "Municipal", greaterAccraId, "Greater Accra", "Weija", 411377, 12341),
                CreateDistrict("22222222-2222-2222-2222-222222220006", "Ga Central Municipal", "GCMA", "Municipal", greaterAccraId, "Greater Accra", "Sowutuom", 117220, 3517),
                CreateDistrict("22222222-2222-2222-2222-222222220007", "Ga North Municipal", "GNMA", "Municipal", greaterAccraId, "Greater Accra", "Ofankor", 155890, 4677),
                CreateDistrict("22222222-2222-2222-2222-222222220008", "La Dade Kotopon Municipal", "LDKMA", "Municipal", greaterAccraId, "Greater Accra", "La", 196801, 5904),
                CreateDistrict("22222222-2222-2222-2222-222222220009", "La Nkwantanang Madina Municipal", "LNMMA", "Municipal", greaterAccraId, "Greater Accra", "Madina", 164726, 4942),
                CreateDistrict("22222222-2222-2222-2222-222222220010", "Ledzokuku Municipal", "LMA", "Municipal", greaterAccraId, "Greater Accra", "Teshie-Nungua", 227932, 6838),
                CreateDistrict("22222222-2222-2222-2222-222222220011", "Kpone Katamanso Municipal", "KKMA", "Municipal", greaterAccraId, "Greater Accra", "Kpone", 118789, 3564),
                CreateDistrict("22222222-2222-2222-2222-222222220012", "Adentan Municipal", "ADMA", "Municipal", greaterAccraId, "Greater Accra", "Adentan", 111135, 3334),
                CreateDistrict("22222222-2222-2222-2222-222222220013", "Ashaiman Municipal", "ASHMA", "Municipal", greaterAccraId, "Greater Accra", "Ashaiman", 190186, 5706),
                CreateDistrict("22222222-2222-2222-2222-222222220014", "Ablekuma Central Municipal", "ACMA", "Municipal", greaterAccraId, "Greater Accra", "Dansoman", 189234, 5677),
                CreateDistrict("22222222-2222-2222-2222-222222220015", "Ablekuma North Municipal", "ANMA", "Municipal", greaterAccraId, "Greater Accra", "Chantan", 198565, 5957),
                CreateDistrict("22222222-2222-2222-2222-222222220016", "Ablekuma West Municipal", "AWMA", "Municipal", greaterAccraId, "Greater Accra", "Ga Mashie", 156000, 4680),
                CreateDistrict("22222222-2222-2222-2222-222222220017", "Korle Klottey Municipal", "KKMU", "Municipal", greaterAccraId, "Greater Accra", "Osu", 100088, 3003),
                CreateDistrict("22222222-2222-2222-2222-222222220018", "Ayawaso Central Municipal", "AYCMA", "Municipal", greaterAccraId, "Greater Accra", "Ayawaso", 89955, 2699),
                CreateDistrict("22222222-2222-2222-2222-222222220019", "Ayawaso East Municipal", "AYEMA", "Municipal", greaterAccraId, "Greater Accra", "Nima", 95986, 2880),
                CreateDistrict("22222222-2222-2222-2222-222222220020", "Ayawaso North Municipal", "AYNMA", "Municipal", greaterAccraId, "Greater Accra", "Tesano", 102356, 3071),
                CreateDistrict("22222222-2222-2222-2222-222222220021", "Ayawaso West Municipal", "AYWMA", "Municipal", greaterAccraId, "Greater Accra", "Abelemkpe", 153098, 4593),
                CreateDistrict("22222222-2222-2222-2222-222222220022", "Okaikwei North Municipal", "ONMA", "Municipal", greaterAccraId, "Greater Accra", "Tesano", 145680, 4370),
                CreateDistrict("22222222-2222-2222-2222-222222220023", "Weija Gbawe Municipal", "WGMA", "Municipal", greaterAccraId, "Greater Accra", "Gbawe", 215247, 6457),
                CreateDistrict("22222222-2222-2222-2222-222222220024", "Ada East District", "AEDA", "District", greaterAccraId, "Greater Accra", "Ada Foah", 81655, 2450),
                CreateDistrict("22222222-2222-2222-2222-222222220025", "Ada West District", "AWDA", "District", greaterAccraId, "Greater Accra", "Sege", 62434, 1873),
                CreateDistrict("22222222-2222-2222-2222-222222220026", "Ningo Prampram District", "NPDA", "District", greaterAccraId, "Greater Accra", "Prampram", 79727, 2392),
                CreateDistrict("22222222-2222-2222-2222-222222220027", "Shai Osudoku District", "SODA", "District", greaterAccraId, "Greater Accra", "Dodowa", 53898, 1617),
            });

            // Ashanti Region Districts
            var ashantiId = Guid.Parse("11111111-1111-1111-1111-111111111102");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220028", "Kumasi Metropolitan", "KMA", "Metropolitan", ashantiId, "Ashanti", "Kumasi", 2035064, 61052),
                CreateDistrict("22222222-2222-2222-2222-222222220029", "Oforikrom Municipal", "OMA", "Municipal", ashantiId, "Ashanti", "Oforikrom", 153716, 4611),
                CreateDistrict("22222222-2222-2222-2222-222222220030", "Asokwa Municipal", "ASKMA", "Municipal", ashantiId, "Ashanti", "Asokwa", 143819, 4315),
                CreateDistrict("22222222-2222-2222-2222-222222220031", "Suame Municipal", "SMA", "Municipal", ashantiId, "Ashanti", "Suame", 149208, 4476),
                CreateDistrict("22222222-2222-2222-2222-222222220032", "Old Tafo Municipal", "OTMA", "Municipal", ashantiId, "Ashanti", "Tafo", 133671, 4010),
                CreateDistrict("22222222-2222-2222-2222-222222220033", "Kwadaso Municipal", "KWMA", "Municipal", ashantiId, "Ashanti", "Kwadaso", 106749, 3202),
                CreateDistrict("22222222-2222-2222-2222-222222220034", "Nhyiaeso Municipal", "NMA", "Municipal", ashantiId, "Ashanti", "Nhyiaeso", 69456, 2084),
                CreateDistrict("22222222-2222-2222-2222-222222220035", "Bantama Municipal", "BMA", "Municipal", ashantiId, "Ashanti", "Bantama", 115320, 3460),
                CreateDistrict("22222222-2222-2222-2222-222222220036", "Asokore Mampong Municipal", "AMMA", "Municipal", ashantiId, "Ashanti", "Asokore Mampong", 224311, 6729),
                CreateDistrict("22222222-2222-2222-2222-222222220037", "Ejisu Municipal", "EMA", "Municipal", ashantiId, "Ashanti", "Ejisu", 168138, 5044),
                CreateDistrict("22222222-2222-2222-2222-222222220038", "Juaben Municipal", "JMA", "Municipal", ashantiId, "Ashanti", "Juaben", 81000, 2430),
                CreateDistrict("22222222-2222-2222-2222-222222220039", "Mampong Municipal", "MMA", "Municipal", ashantiId, "Ashanti", "Mampong", 80756, 2423),
                CreateDistrict("22222222-2222-2222-2222-222222220040", "Offinso North District", "ONDA", "District", ashantiId, "Ashanti", "Akomadan", 62565, 1877),
                CreateDistrict("22222222-2222-2222-2222-222222220041", "Offinso Municipal", "OFMA", "Municipal", ashantiId, "Ashanti", "Offinso", 89972, 2699),
                CreateDistrict("22222222-2222-2222-2222-222222220042", "Afigya Kwabre South District", "AKSDA", "District", ashantiId, "Ashanti", "Kodie", 115520, 3466),
                CreateDistrict("22222222-2222-2222-2222-222222220043", "Afigya Kwabre North District", "AKNDA", "District", ashantiId, "Ashanti", "Afrancho", 85678, 2570),
                CreateDistrict("22222222-2222-2222-2222-222222220044", "Kwabre East Municipal", "KEMA", "Municipal", ashantiId, "Ashanti", "Mamponteng", 141867, 4256),
                CreateDistrict("22222222-2222-2222-2222-222222220045", "Atwima Kwanwoma District", "AKDA", "District", ashantiId, "Ashanti", "Foase", 122905, 3687),
                CreateDistrict("22222222-2222-2222-2222-222222220046", "Atwima Nwabiagya North District", "ANNDA", "District", ashantiId, "Ashanti", "Barekese", 98743, 2962),
                CreateDistrict("22222222-2222-2222-2222-222222220047", "Atwima Nwabiagya Municipal", "ANWMA", "Municipal", ashantiId, "Ashanti", "Nkawie", 161188, 4836),
                CreateDistrict("22222222-2222-2222-2222-222222220048", "Atwima Mponua District", "AMDA", "District", ashantiId, "Ashanti", "Nyinahin", 131892, 3957),
                CreateDistrict("22222222-2222-2222-2222-222222220049", "Amansie Central District", "ACDA", "District", ashantiId, "Ashanti", "Jacobu", 99986, 3000),
                CreateDistrict("22222222-2222-2222-2222-222222220050", "Amansie South District", "ASDA", "District", ashantiId, "Ashanti", "Manso Nkwanta", 137871, 4136),
                CreateDistrict("22222222-2222-2222-2222-222222220051", "Amansie West District", "AWDIST", "District", ashantiId, "Ashanti", "Manso Nkwanta", 152448, 4573),
                CreateDistrict("22222222-2222-2222-2222-222222220052", "Bekwai Municipal", "BKMA", "Municipal", ashantiId, "Ashanti", "Bekwai", 135924, 4078),
                CreateDistrict("22222222-2222-2222-2222-222222220053", "Bosome Freho District", "BFDA", "District", ashantiId, "Ashanti", "Asiwa", 59635, 1789),
                CreateDistrict("22222222-2222-2222-2222-222222220054", "Adansi Asokwa District", "AASDA", "District", ashantiId, "Ashanti", "Obuasi", 96500, 2895),
                CreateDistrict("22222222-2222-2222-2222-222222220055", "Adansi North District", "ANDA", "District", ashantiId, "Ashanti", "Fomena", 110924, 3328),
                CreateDistrict("22222222-2222-2222-2222-222222220056", "Adansi South District", "ASDIST", "District", ashantiId, "Ashanti", "New Edubiase", 129929, 3898),
                CreateDistrict("22222222-2222-2222-2222-222222220057", "Obuasi Municipal", "OBMA", "Municipal", ashantiId, "Ashanti", "Obuasi", 175043, 5251),
                CreateDistrict("22222222-2222-2222-2222-222222220058", "Obuasi East District", "OEDA", "District", ashantiId, "Ashanti", "Tutuka", 68765, 2063),
                CreateDistrict("22222222-2222-2222-2222-222222220059", "Asante Akim North Municipal", "AANMA", "Municipal", ashantiId, "Ashanti", "Konongo", 75928, 2278),
                CreateDistrict("22222222-2222-2222-2222-222222220060", "Asante Akim Central Municipal", "AACMA", "Municipal", ashantiId, "Ashanti", "Konongo", 81374, 2441),
                CreateDistrict("22222222-2222-2222-2222-222222220061", "Asante Akim South Municipal", "AASMA", "Municipal", ashantiId, "Ashanti", "Juaso", 123135, 3694),
                CreateDistrict("22222222-2222-2222-2222-222222220062", "Sekyere South District", "SSDA", "District", ashantiId, "Ashanti", "Agona", 105938, 3178),
                CreateDistrict("22222222-2222-2222-2222-222222220063", "Sekyere Central District", "SCDA", "District", ashantiId, "Ashanti", "Nsuta", 71758, 2153),
                CreateDistrict("22222222-2222-2222-2222-222222220064", "Sekyere East District", "SEDA", "District", ashantiId, "Ashanti", "Effiduase", 96350, 2891),
                CreateDistrict("22222222-2222-2222-2222-222222220065", "Sekyere Kumawu District", "SKDA", "District", ashantiId, "Ashanti", "Kumawu", 62871, 1886),
                CreateDistrict("22222222-2222-2222-2222-222222220066", "Sekyere Afram Plains District", "SAPDA", "District", ashantiId, "Ashanti", "Drobonso", 85000, 2550),
                CreateDistrict("22222222-2222-2222-2222-222222220067", "Bosomtwe District", "BODA", "District", ashantiId, "Ashanti", "Kuntanase", 103746, 3112),
                CreateDistrict("22222222-2222-2222-2222-222222220068", "Ahafo Ano North Municipal", "AANMU", "Municipal", ashantiId, "Ashanti", "Tepa", 102139, 3064),
                CreateDistrict("22222222-2222-2222-2222-222222220069", "Ahafo Ano South East District", "AASEDA", "District", ashantiId, "Ashanti", "Mankranso", 81250, 2438),
                CreateDistrict("22222222-2222-2222-2222-222222220070", "Ahafo Ano South West District", "AASWDA", "District", ashantiId, "Ashanti", "Kunsu", 75680, 2270),
            });

            // Western Region Districts
            var westernId = Guid.Parse("11111111-1111-1111-1111-111111111103");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220071", "Sekondi Takoradi Metropolitan", "STMA", "Metropolitan", westernId, "Western", "Sekondi", 559548, 16786),
                CreateDistrict("22222222-2222-2222-2222-222222220072", "Effia-Kwesimintsim Municipal", "EKMA", "Municipal", westernId, "Western", "Kwesimintsim", 189000, 5670),
                CreateDistrict("22222222-2222-2222-2222-222222220073", "Essikado-Ketan Municipal", "EKEMA", "Municipal", westernId, "Western", "Essikado", 125000, 3750),
                CreateDistrict("22222222-2222-2222-2222-222222220074", "Ahanta West Municipal", "AHWMA", "Municipal", westernId, "Western", "Agona Nkwanta", 123791, 3714),
                CreateDistrict("22222222-2222-2222-2222-222222220075", "Shama District", "SHDA", "District", westernId, "Western", "Shama", 95430, 2863),
                CreateDistrict("22222222-2222-2222-2222-222222220076", "Wassa East District", "WEDA", "District", westernId, "Western", "Daboase", 95830, 2875),
                CreateDistrict("22222222-2222-2222-2222-222222220077", "Mpohor District", "MPDA", "District", westernId, "Western", "Mpohor", 58553, 1757),
                CreateDistrict("22222222-2222-2222-2222-222222220078", "Tarkwa Nsuaem Municipal", "TNMA", "Municipal", westernId, "Western", "Tarkwa", 103593, 3108),
                CreateDistrict("22222222-2222-2222-2222-222222220079", "Prestea Huni Valley Municipal", "PHVMA", "Municipal", westernId, "Western", "Bogoso", 195720, 5872),
                CreateDistrict("22222222-2222-2222-2222-222222220080", "Wassa Amenfi East Municipal", "WAEMA", "Municipal", westernId, "Western", "Wassa Akropong", 98413, 2952),
                CreateDistrict("22222222-2222-2222-2222-222222220081", "Wassa Amenfi West Municipal", "WAWMA", "Municipal", westernId, "Western", "Asankragua", 145671, 4370),
                CreateDistrict("22222222-2222-2222-2222-222222220082", "Wassa Amenfi Central District", "WACDA", "District", westernId, "Western", "Manso Amenfi", 89000, 2670),
                CreateDistrict("22222222-2222-2222-2222-222222220083", "Ellembele District", "ELDA", "District", westernId, "Western", "Nkroful", 103325, 3100),
                CreateDistrict("22222222-2222-2222-2222-222222220084", "Jomoro Municipal", "JOMA", "Municipal", westernId, "Western", "Half Assini", 178635, 5359),
                CreateDistrict("22222222-2222-2222-2222-222222220085", "Nzema East Municipal", "NEMA", "Municipal", westernId, "Western", "Axim", 103076, 3092),
            });

            // Central Region Districts
            var centralId = Guid.Parse("11111111-1111-1111-1111-111111111104");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220086", "Cape Coast Metropolitan", "CCMA", "Metropolitan", centralId, "Central", "Cape Coast", 169894, 5097),
                CreateDistrict("22222222-2222-2222-2222-222222220087", "Komenda Edina Eguafo Abirem Municipal", "KEEMA", "Municipal", centralId, "Central", "Elmina", 167846, 5035),
                CreateDistrict("22222222-2222-2222-2222-222222220088", "Abura Asebu Kwamankese District", "AAKDA", "District", centralId, "Central", "Abura Dunkwa", 133065, 3992),
                CreateDistrict("22222222-2222-2222-2222-222222220089", "Mfantsiman Municipal", "MFMA", "Municipal", centralId, "Central", "Saltpond", 201844, 6055),
                CreateDistrict("22222222-2222-2222-2222-222222220090", "Ekumfi District", "EKDA", "District", centralId, "Central", "Essarkyir", 55349, 1660),
                CreateDistrict("22222222-2222-2222-2222-222222220091", "Ajumako Enyan Essiam District", "AEEDA", "District", centralId, "Central", "Ajumako", 144015, 4320),
                CreateDistrict("22222222-2222-2222-2222-222222220092", "Gomoa West District", "GWDA", "District", centralId, "Central", "Apam", 169501, 5085),
                CreateDistrict("22222222-2222-2222-2222-222222220093", "Gomoa East District", "GEDA", "District", centralId, "Central", "Potsin", 159000, 4770),
                CreateDistrict("22222222-2222-2222-2222-222222220094", "Gomoa Central District", "GCDA", "District", centralId, "Central", "Afransi", 125000, 3750),
                CreateDistrict("22222222-2222-2222-2222-222222220095", "Effutu Municipal", "EFMA", "Municipal", centralId, "Central", "Winneba", 89807, 2694),
                CreateDistrict("22222222-2222-2222-2222-222222220096", "Awutu Senya District", "ASDA1", "District", centralId, "Central", "Awutu Breku", 116820, 3505),
                CreateDistrict("22222222-2222-2222-2222-222222220097", "Awutu Senya East Municipal", "ASEMA", "Municipal", centralId, "Central", "Kasoa", 231015, 6930),
                CreateDistrict("22222222-2222-2222-2222-222222220098", "Agona East District", "AEDA1", "District", centralId, "Central", "Nsaba", 82341, 2470),
                CreateDistrict("22222222-2222-2222-2222-222222220099", "Agona West Municipal", "AGWMA", "Municipal", centralId, "Central", "Agona Swedru", 152638, 4579),
                CreateDistrict("22222222-2222-2222-2222-222222220100", "Asikuma Odoben Brakwa District", "AOBDA", "District", centralId, "Central", "Breman Asikuma", 101400, 3042),
                CreateDistrict("22222222-2222-2222-2222-222222220101", "Assin North Municipal", "ASNMA", "Municipal", centralId, "Central", "Assin Foso", 177572, 5327),
                CreateDistrict("22222222-2222-2222-2222-222222220102", "Assin South District", "ASSDA", "District", centralId, "Central", "Nsuaem Kyekyewere", 125649, 3769),
                CreateDistrict("22222222-2222-2222-2222-222222220103", "Twifo Atti Morkwa District", "TAMDA", "District", centralId, "Central", "Twifo Praso", 95822, 2875),
                CreateDistrict("22222222-2222-2222-2222-222222220104", "Hemang Lower Denkyira District", "HLDDA", "District", centralId, "Central", "Hemang", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220105", "Upper Denkyira East Municipal", "UDEMA", "Municipal", centralId, "Central", "Dunkwa-on-Offin", 131858, 3956),
                CreateDistrict("22222222-2222-2222-2222-222222220106", "Upper Denkyira West District", "UDWDA", "District", centralId, "Central", "Diaso", 85000, 2550),
            });

            // Eastern Region Districts
            var easternId = Guid.Parse("11111111-1111-1111-1111-111111111105");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220107", "New Juaben South Municipal", "NJSMA", "Municipal", easternId, "Eastern", "Koforidua", 183727, 5512),
                CreateDistrict("22222222-2222-2222-2222-222222220108", "New Juaben North Municipal", "NJNMA", "Municipal", easternId, "Eastern", "Effiduase", 111000, 3330),
                CreateDistrict("22222222-2222-2222-2222-222222220109", "Nsawam Adoagyiri Municipal", "NAMA", "Municipal", easternId, "Eastern", "Nsawam", 143000, 4290),
                CreateDistrict("22222222-2222-2222-2222-222222220110", "Akuapem South District", "AKSDA1", "District", easternId, "Eastern", "Aburi", 77686, 2331),
                CreateDistrict("22222222-2222-2222-2222-222222220111", "Akuapem North Municipal", "AKNMA", "Municipal", easternId, "Eastern", "Akropong", 89850, 2696),
                CreateDistrict("22222222-2222-2222-2222-222222220112", "Okere District", "OKDA", "District", easternId, "Eastern", "Adukrom", 59000, 1770),
                CreateDistrict("22222222-2222-2222-2222-222222220113", "East Akim Municipal", "EAMA", "Municipal", easternId, "Eastern", "Kibi", 192171, 5765),
                CreateDistrict("22222222-2222-2222-2222-222222220114", "West Akim Municipal", "WAMA", "Municipal", easternId, "Eastern", "Asamankese", 193767, 5813),
                CreateDistrict("22222222-2222-2222-2222-222222220115", "Birim North District", "BNDA", "District", easternId, "Eastern", "New Abirem", 109793, 3294),
                CreateDistrict("22222222-2222-2222-2222-222222220116", "Birim Central Municipal", "BCMA", "Municipal", easternId, "Eastern", "Oda", 192855, 5786),
                CreateDistrict("22222222-2222-2222-2222-222222220117", "Birim South District", "BSDA", "District", easternId, "Eastern", "Akim Swedru", 98000, 2940),
                CreateDistrict("22222222-2222-2222-2222-222222220118", "Achiase District", "ACHDA", "District", easternId, "Eastern", "Achiase", 78000, 2340),
                CreateDistrict("22222222-2222-2222-2222-222222220119", "Denkyembour District", "DENDA", "District", easternId, "Eastern", "Akwatia", 82000, 2460),
                CreateDistrict("22222222-2222-2222-2222-222222220120", "Kwahu West Municipal", "KWWMA", "Municipal", easternId, "Eastern", "Nkawkaw", 154527, 4636),
                CreateDistrict("22222222-2222-2222-2222-222222220121", "Kwahu East District", "KWEDA", "District", easternId, "Eastern", "Abetifi", 80126, 2404),
                CreateDistrict("22222222-2222-2222-2222-222222220122", "Kwahu South District", "KWSDA", "District", easternId, "Eastern", "Mpraeso", 72000, 2160),
                CreateDistrict("22222222-2222-2222-2222-222222220123", "Kwahu Afram Plains South District", "KAPSDA", "District", easternId, "Eastern", "Donkorkrom", 132000, 3960),
                CreateDistrict("22222222-2222-2222-2222-222222220124", "Kwahu Afram Plains North District", "KAPNDA", "District", easternId, "Eastern", "Donkorkrom", 87674, 2630),
                CreateDistrict("22222222-2222-2222-2222-222222220125", "Abuakwa North Municipal", "ABNMA", "Municipal", easternId, "Eastern", "Kukurantumi", 156000, 4680),
                CreateDistrict("22222222-2222-2222-2222-222222220126", "Abuakwa South Municipal", "ABSMA", "Municipal", easternId, "Eastern", "Kibi", 125000, 3750),
                CreateDistrict("22222222-2222-2222-2222-222222220127", "Atiwa East District", "ATEDA", "District", easternId, "Eastern", "Anyinam", 98000, 2940),
                CreateDistrict("22222222-2222-2222-2222-222222220128", "Atiwa West District", "ATWDA", "District", easternId, "Eastern", "Kwabeng", 85000, 2550),
                CreateDistrict("22222222-2222-2222-2222-222222220129", "Fanteakwa North District", "FNDA", "District", easternId, "Eastern", "Begoro", 69000, 2070),
                CreateDistrict("22222222-2222-2222-2222-222222220130", "Fanteakwa South District", "FSDA", "District", easternId, "Eastern", "Osino", 62000, 1860),
                CreateDistrict("22222222-2222-2222-2222-222222220131", "Yilo Krobo Municipal", "YKMA", "Municipal", easternId, "Eastern", "Somanya", 110252, 3308),
                CreateDistrict("22222222-2222-2222-2222-222222220132", "Lower Manya Krobo Municipal", "LMKMA", "Municipal", easternId, "Eastern", "Odumase", 102000, 3060),
                CreateDistrict("22222222-2222-2222-2222-222222220133", "Upper Manya Krobo District", "UMKDA", "District", easternId, "Eastern", "Asesewa", 73000, 2190),
                CreateDistrict("22222222-2222-2222-2222-222222220134", "Ayensuano District", "AYDA", "District", easternId, "Eastern", "Coaltar", 89000, 2670),
            });

            // Volta Region Districts
            var voltaId = Guid.Parse("11111111-1111-1111-1111-111111111106");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220135", "Ho Municipal", "HOMA", "Municipal", voltaId, "Volta", "Ho", 194104, 5823),
                CreateDistrict("22222222-2222-2222-2222-222222220136", "Ho West District", "HWDA", "District", voltaId, "Volta", "Dzolokpuita", 104454, 3134),
                CreateDistrict("22222222-2222-2222-2222-222222220137", "South Dayi District", "SDDA", "District", voltaId, "Volta", "Kpeve", 54605, 1638),
                CreateDistrict("22222222-2222-2222-2222-222222220138", "North Dayi District", "NDDA", "District", voltaId, "Volta", "Anfoega", 47333, 1420),
                CreateDistrict("22222222-2222-2222-2222-222222220139", "Akatsi South District", "ASDA2", "District", voltaId, "Volta", "Akatsi", 115629, 3469),
                CreateDistrict("22222222-2222-2222-2222-222222220140", "Akatsi North District", "ANDA1", "District", voltaId, "Volta", "Ave Dakpa", 57000, 1710),
                CreateDistrict("22222222-2222-2222-2222-222222220141", "Ketu South Municipal", "KSMA", "Municipal", voltaId, "Volta", "Denu", 198433, 5953),
                CreateDistrict("22222222-2222-2222-2222-222222220142", "Ketu North Municipal", "KNMA", "Municipal", voltaId, "Volta", "Dzodze", 104800, 3144),
                CreateDistrict("22222222-2222-2222-2222-222222220143", "Keta Municipal", "KETMA", "Municipal", voltaId, "Volta", "Keta", 160756, 4823),
                CreateDistrict("22222222-2222-2222-2222-222222220144", "Anloga District", "ANLDA", "District", voltaId, "Volta", "Anloga", 85000, 2550),
                CreateDistrict("22222222-2222-2222-2222-222222220145", "South Tongu District", "STDA", "District", voltaId, "Volta", "Sogakope", 112166, 3365),
                CreateDistrict("22222222-2222-2222-2222-222222220146", "Central Tongu District", "CTDA", "District", voltaId, "Volta", "Adidome", 73000, 2190),
                CreateDistrict("22222222-2222-2222-2222-222222220147", "North Tongu District", "NTDA", "District", voltaId, "Volta", "Battor", 85999, 2580),
                CreateDistrict("22222222-2222-2222-2222-222222220148", "Adaklu District", "ADADA", "District", voltaId, "Volta", "Adaklu Waya", 40000, 1200),
                CreateDistrict("22222222-2222-2222-2222-222222220149", "Agotime Ziope District", "AZDA", "District", voltaId, "Volta", "Kpetoe", 45000, 1350),
                CreateDistrict("22222222-2222-2222-2222-222222220150", "Afadzato South District", "AFSDA", "District", voltaId, "Volta", "Ve Golokwati", 65000, 1950),
            });

            // Oti Region Districts
            var otiId = Guid.Parse("11111111-1111-1111-1111-111111111107");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220151", "Krachi East Municipal", "KREMA", "Municipal", otiId, "Oti", "Dambai", 154513, 4635),
                CreateDistrict("22222222-2222-2222-2222-222222220152", "Krachi West District", "KRWDA", "District", otiId, "Oti", "Kete Krachi", 68000, 2040),
                CreateDistrict("22222222-2222-2222-2222-222222220153", "Krachi Nchumuru District", "KNDA", "District", otiId, "Oti", "Chinderi", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220154", "Nkwanta South Municipal", "NKSMA", "Municipal", otiId, "Oti", "Nkwanta", 157621, 4729),
                CreateDistrict("22222222-2222-2222-2222-222222220155", "Nkwanta North District", "NKNDA", "District", otiId, "Oti", "Kpassa", 77000, 2310),
                CreateDistrict("22222222-2222-2222-2222-222222220156", "Biakoye District", "BIADA", "District", otiId, "Oti", "Nkonya Ahenkro", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220157", "Jasikan District", "JASDA", "District", otiId, "Oti", "Jasikan", 62665, 1880),
                CreateDistrict("22222222-2222-2222-2222-222222220158", "Kadjebi District", "KADDA", "District", otiId, "Oti", "Kadjebi", 65000, 1950),
            });

            // Northern Region Districts
            var northernId = Guid.Parse("11111111-1111-1111-1111-111111111108");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220159", "Tamale Metropolitan", "TAMA", "Metropolitan", northernId, "Northern", "Tamale", 374744, 11242),
                CreateDistrict("22222222-2222-2222-2222-222222220160", "Sagnarigu Municipal", "SAGMA", "Municipal", northernId, "Northern", "Sagnarigu", 320000, 9600),
                CreateDistrict("22222222-2222-2222-2222-222222220161", "Tolon District", "TOLDA", "District", northernId, "Northern", "Tolon", 110000, 3300),
                CreateDistrict("22222222-2222-2222-2222-222222220162", "Kumbungu District", "KUMDA", "District", northernId, "Northern", "Kumbungu", 50000, 1500),
                CreateDistrict("22222222-2222-2222-2222-222222220163", "Savelugu Municipal", "SAVMA", "Municipal", northernId, "Northern", "Savelugu", 154000, 4620),
                CreateDistrict("22222222-2222-2222-2222-222222220164", "Nanton District", "NANDA", "District", northernId, "Northern", "Nanton", 55000, 1650),
                CreateDistrict("22222222-2222-2222-2222-222222220165", "Mion District", "MIODA", "District", northernId, "Northern", "Sang", 82000, 2460),
                CreateDistrict("22222222-2222-2222-2222-222222220166", "Yendi Municipal", "YENMA", "Municipal", northernId, "Northern", "Yendi", 180000, 5400),
                CreateDistrict("22222222-2222-2222-2222-222222220167", "Zabzugu District", "ZABDA", "District", northernId, "Northern", "Zabzugu", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220168", "Tatale Sanguli District", "TSDA", "District", northernId, "Northern", "Tatale", 55000, 1650),
                CreateDistrict("22222222-2222-2222-2222-222222220169", "Gushegu Municipal", "GUSMA", "Municipal", northernId, "Northern", "Gushegu", 128000, 3840),
                CreateDistrict("22222222-2222-2222-2222-222222220170", "Karaga District", "KARDA", "District", northernId, "Northern", "Karaga", 85000, 2550),
                CreateDistrict("22222222-2222-2222-2222-222222220171", "Nanumba North Municipal", "NNMA", "Municipal", northernId, "Northern", "Bimbilla", 141000, 4230),
                CreateDistrict("22222222-2222-2222-2222-222222220172", "Nanumba South District", "NSDA", "District", northernId, "Northern", "Wulensi", 93000, 2790),
                CreateDistrict("22222222-2222-2222-2222-222222220173", "Kpandai District", "KPADA", "District", northernId, "Northern", "Kpandai", 100000, 3000),
            });

            // Savannah Region Districts
            var savannahId = Guid.Parse("11111111-1111-1111-1111-111111111109");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220174", "West Gonja Municipal", "WGOMA", "Municipal", savannahId, "Savannah", "Damongo", 97224, 2917),
                CreateDistrict("22222222-2222-2222-2222-222222220175", "Central Gonja District", "CGDA", "District", savannahId, "Savannah", "Buipe", 90000, 2700),
                CreateDistrict("22222222-2222-2222-2222-222222220176", "East Gonja Municipal", "EGMA", "Municipal", savannahId, "Savannah", "Salaga", 117473, 3524),
                CreateDistrict("22222222-2222-2222-2222-222222220177", "North Gonja District", "NGDA", "District", savannahId, "Savannah", "Daboya", 70000, 2100),
                CreateDistrict("22222222-2222-2222-2222-222222220178", "North East Gonja District", "NEGDA", "District", savannahId, "Savannah", "Kpalbe", 60000, 1800),
                CreateDistrict("22222222-2222-2222-2222-222222220179", "Bole District", "BOLDA", "District", savannahId, "Savannah", "Bole", 80000, 2400),
                CreateDistrict("22222222-2222-2222-2222-222222220180", "Sawla Tuna Kalba District", "STKDA", "District", savannahId, "Savannah", "Sawla", 110000, 3300),
            });

            // North East Region Districts
            var northEastId = Guid.Parse("11111111-1111-1111-1111-111111111110");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220181", "West Mamprusi Municipal", "WMMA", "Municipal", northEastId, "North East", "Walewale", 168000, 5040),
                CreateDistrict("22222222-2222-2222-2222-222222220182", "East Mamprusi Municipal", "EMMA", "Municipal", northEastId, "North East", "Nalerigu", 170000, 5100),
                CreateDistrict("22222222-2222-2222-2222-222222220183", "Mamprugu Moagduri District", "MMDA", "District", northEastId, "North East", "Yagaba", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220184", "Yunyoo Nasuan District", "YNDA", "District", northEastId, "North East", "Yunyoo", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220185", "Chereponi District", "CHEDA", "District", northEastId, "North East", "Chereponi", 70000, 2100),
                CreateDistrict("22222222-2222-2222-2222-222222220186", "Bunkpurugu Nakpanduri District", "BNDA1", "District", northEastId, "North East", "Bunkpurugu", 95000, 2850),
            });

            // Upper East Region Districts
            var upperEastId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220187", "Bolgatanga Municipal", "BOLMA", "Municipal", upperEastId, "Upper East", "Bolgatanga", 131550, 3947),
                CreateDistrict("22222222-2222-2222-2222-222222220188", "Bolgatanga East District", "BEDA", "District", upperEastId, "Upper East", "Zuarungu", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220189", "Bongo District", "BONDA", "District", upperEastId, "Upper East", "Bongo", 84545, 2536),
                CreateDistrict("22222222-2222-2222-2222-222222220190", "Nabdam District", "NABDA", "District", upperEastId, "Upper East", "Nangodi", 38989, 1170),
                CreateDistrict("22222222-2222-2222-2222-222222220191", "Talensi District", "TALDA", "District", upperEastId, "Upper East", "Tongo", 81000, 2430),
                CreateDistrict("22222222-2222-2222-2222-222222220192", "Bawku West District", "BWWDA", "District", upperEastId, "Upper East", "Zebilla", 100000, 3000),
                CreateDistrict("22222222-2222-2222-2222-222222220193", "Bawku Municipal", "BAWMA", "Municipal", upperEastId, "Upper East", "Bawku", 225000, 6750),
                CreateDistrict("22222222-2222-2222-2222-222222220194", "Binduri District", "BINDA", "District", upperEastId, "Upper East", "Binduri", 70000, 2100),
                CreateDistrict("22222222-2222-2222-2222-222222220195", "Pusiga District", "PUSDA", "District", upperEastId, "Upper East", "Pusiga", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220196", "Garu District", "GARDA", "District", upperEastId, "Upper East", "Garu", 90000, 2700),
                CreateDistrict("22222222-2222-2222-2222-222222220197", "Tempane District", "TEMDA", "District", upperEastId, "Upper East", "Tempane", 72000, 2160),
                CreateDistrict("22222222-2222-2222-2222-222222220198", "Builsa North Municipal", "BUNMA", "Municipal", upperEastId, "Upper East", "Sandema", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220199", "Builsa South District", "BUSDA", "District", upperEastId, "Upper East", "Fumbisi", 45000, 1350),
                CreateDistrict("22222222-2222-2222-2222-222222220200", "Kassena Nankana West District", "KNWDA", "District", upperEastId, "Upper East", "Paga", 82000, 2460),
                CreateDistrict("22222222-2222-2222-2222-222222220201", "Kassena Nankana Municipal", "KNMA1", "Municipal", upperEastId, "Upper East", "Navrongo", 115000, 3450),
            });

            // Upper West Region Districts
            var upperWestId = Guid.Parse("11111111-1111-1111-1111-111111111112");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220202", "Wa Municipal", "WAMA1", "Municipal", upperWestId, "Upper West", "Wa", 127284, 3819),
                CreateDistrict("22222222-2222-2222-2222-222222220203", "Wa East District", "WAEDA", "District", upperWestId, "Upper West", "Funsi", 90000, 2700),
                CreateDistrict("22222222-2222-2222-2222-222222220204", "Wa West District", "WAWDA", "District", upperWestId, "Upper West", "Wechiau", 85000, 2550),
                CreateDistrict("22222222-2222-2222-2222-222222220205", "Nadowli Kaleo District", "NKDA", "District", upperWestId, "Upper West", "Nadowli", 71000, 2130),
                CreateDistrict("22222222-2222-2222-2222-222222220206", "Daffiama Bussie Issa District", "DBIDA", "District", upperWestId, "Upper West", "Issa", 40000, 1200),
                CreateDistrict("22222222-2222-2222-2222-222222220207", "Jirapa Municipal", "JIRMA", "Municipal", upperWestId, "Upper West", "Jirapa", 97000, 2910),
                CreateDistrict("22222222-2222-2222-2222-222222220208", "Lambussie Karni District", "LKDA", "District", upperWestId, "Upper West", "Lambussie", 55000, 1650),
                CreateDistrict("22222222-2222-2222-2222-222222220209", "Lawra Municipal", "LAWMA", "Municipal", upperWestId, "Upper West", "Lawra", 102000, 3060),
                CreateDistrict("22222222-2222-2222-2222-222222220210", "Nandom Municipal", "NANMA", "Municipal", upperWestId, "Upper West", "Nandom", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220211", "Sissala East Municipal", "SEMA", "Municipal", upperWestId, "Upper West", "Tumu", 71000, 2130),
                CreateDistrict("22222222-2222-2222-2222-222222220212", "Sissala West District", "SWDA", "District", upperWestId, "Upper West", "Gwollu", 55000, 1650),
            });

            // Bono Region Districts
            var bonoId = Guid.Parse("11111111-1111-1111-1111-111111111113");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220213", "Sunyani Municipal", "SUNMA", "Municipal", bonoId, "Bono", "Sunyani", 150000, 4500),
                CreateDistrict("22222222-2222-2222-2222-222222220214", "Sunyani West District", "SUWDA", "District", bonoId, "Bono", "Odomase", 88000, 2640),
                CreateDistrict("22222222-2222-2222-2222-222222220215", "Berekum East Municipal", "BEEMA", "Municipal", bonoId, "Bono", "Berekum", 150000, 4500),
                CreateDistrict("22222222-2222-2222-2222-222222220216", "Berekum West District", "BEWDA", "District", bonoId, "Bono", "Jinijini", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220217", "Dormaa Municipal", "DORMA", "Municipal", bonoId, "Bono", "Dormaa Ahenkro", 140000, 4200),
                CreateDistrict("22222222-2222-2222-2222-222222220218", "Dormaa East District", "DOEDA", "District", bonoId, "Bono", "Wamfie", 65000, 1950),
                CreateDistrict("22222222-2222-2222-2222-222222220219", "Dormaa West District", "DOWDA", "District", bonoId, "Bono", "Nkrankwanta", 55000, 1650),
                CreateDistrict("22222222-2222-2222-2222-222222220220", "Jaman North District", "JANDA", "District", bonoId, "Bono", "Sampa", 95000, 2850),
                CreateDistrict("22222222-2222-2222-2222-222222220221", "Jaman South Municipal", "JASMA", "Municipal", bonoId, "Bono", "Drobo", 100000, 3000),
                CreateDistrict("22222222-2222-2222-2222-222222220222", "Tain District", "TAIDA", "District", bonoId, "Bono", "Nsawkaw", 110000, 3300),
                CreateDistrict("22222222-2222-2222-2222-222222220223", "Wenchi Municipal", "WENMA", "Municipal", bonoId, "Bono", "Wenchi", 97000, 2910),
                CreateDistrict("22222222-2222-2222-2222-222222220224", "Banda District", "BANDA", "District", bonoId, "Bono", "Banda Ahenkro", 35000, 1050),
            });

            // Bono East Region Districts
            var bonoEastId = Guid.Parse("11111111-1111-1111-1111-111111111114");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220225", "Techiman Municipal", "TECMA", "Municipal", bonoEastId, "Bono East", "Techiman", 148000, 4440),
                CreateDistrict("22222222-2222-2222-2222-222222220226", "Techiman North District", "TECNDA", "District", bonoEastId, "Bono East", "Tuobodom", 90000, 2700),
                CreateDistrict("22222222-2222-2222-2222-222222220227", "Nkoranza North District", "NKNDA1", "District", bonoEastId, "Bono East", "Busunya", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220228", "Nkoranza South Municipal", "NKSMA1", "Municipal", bonoEastId, "Bono East", "Nkoranza", 100000, 3000),
                CreateDistrict("22222222-2222-2222-2222-222222220229", "Kintampo North Municipal", "KINMA", "Municipal", bonoEastId, "Bono East", "Kintampo", 125000, 3750),
                CreateDistrict("22222222-2222-2222-2222-222222220230", "Kintampo South District", "KISDA", "District", bonoEastId, "Bono East", "Jema", 95000, 2850),
                CreateDistrict("22222222-2222-2222-2222-222222220231", "Atebubu Amantin Municipal", "AAMA", "Municipal", bonoEastId, "Bono East", "Atebubu", 130000, 3900),
                CreateDistrict("22222222-2222-2222-2222-222222220232", "Sene East District", "SEEDA", "District", bonoEastId, "Bono East", "Kajaji", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220233", "Sene West District", "SEWDA", "District", bonoEastId, "Bono East", "Kwame Danso", 110000, 3300),
                CreateDistrict("22222222-2222-2222-2222-222222220234", "Pru East District", "PREDA", "District", bonoEastId, "Bono East", "Yeji", 90000, 2700),
                CreateDistrict("22222222-2222-2222-2222-222222220235", "Pru West District", "PRWDA", "District", bonoEastId, "Bono East", "Prang", 85000, 2550),
            });

            // Ahafo Region Districts
            var ahafoId = Guid.Parse("11111111-1111-1111-1111-111111111115");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220236", "Asunafo North Municipal", "ASNMA1", "Municipal", ahafoId, "Ahafo", "Goaso", 130000, 3900),
                CreateDistrict("22222222-2222-2222-2222-222222220237", "Asunafo South District", "ASSDA1", "District", ahafoId, "Ahafo", "Kukuom", 100000, 3000),
                CreateDistrict("22222222-2222-2222-2222-222222220238", "Asutifi North District", "ASTNDA", "District", ahafoId, "Ahafo", "Kenyasi", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220239", "Asutifi South District", "ASTSDA", "District", ahafoId, "Ahafo", "Hwidiem", 80000, 2400),
                CreateDistrict("22222222-2222-2222-2222-222222220240", "Tano North Municipal", "TANOMA", "Municipal", ahafoId, "Ahafo", "Duayaw Nkwanta", 95000, 2850),
                CreateDistrict("22222222-2222-2222-2222-222222220241", "Tano South Municipal", "TASOMA", "Municipal", ahafoId, "Ahafo", "Bechem", 85000, 2550),
            });

            // Western North Region Districts
            var westernNorthId = Guid.Parse("11111111-1111-1111-1111-111111111116");
            districts.AddRange(new[]
            {
                CreateDistrict("22222222-2222-2222-2222-222222220242", "Sefwi Wiawso Municipal", "SWWMA", "Municipal", westernNorthId, "Western North", "Wiawso", 180000, 5400),
                CreateDistrict("22222222-2222-2222-2222-222222220243", "Sefwi Akontombra District", "SAKDA", "District", westernNorthId, "Western North", "Akontombra", 90000, 2700),
                CreateDistrict("22222222-2222-2222-2222-222222220244", "Sefwi Bibiani Anhwiaso Bekwai Municipal", "SBABMA", "Municipal", westernNorthId, "Western North", "Bibiani", 170000, 5100),
                CreateDistrict("22222222-2222-2222-2222-222222220245", "Juaboso District", "JUADA", "District", westernNorthId, "Western North", "Juaboso", 120000, 3600),
                CreateDistrict("22222222-2222-2222-2222-222222220246", "Bia East District", "BIEDA", "District", westernNorthId, "Western North", "Adabokrom", 75000, 2250),
                CreateDistrict("22222222-2222-2222-2222-222222220247", "Bia West District", "BIWDA", "District", westernNorthId, "Western North", "Essam Debiso", 115000, 3450),
                CreateDistrict("22222222-2222-2222-2222-222222220248", "Bodi District", "BODDA", "District", westernNorthId, "Western North", "Bodi", 85000, 2550),
                CreateDistrict("22222222-2222-2222-2222-222222220249", "Suaman District", "SUADA", "District", westernNorthId, "Western North", "Dadieso", 75000, 2250),
            });

            return districts;
        }

        private District CreateDistrict(string id, string name, string code, string type, Guid regionId, string regionName, string capital, int population, int expectedDeliveries)
        {
            var now = DateTime.UtcNow;
            return new District
            {
                ID = Guid.Parse(id),
                Name = name,
                Code = code,
                Type = type,
                RegionID = regionId,
                Region = new Region
                {
                    ID = regionId,
                    Name = regionName,
                    Code = regionId.ToString().Substring(0, 8).ToUpper(),
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Deleted = 0
                }, 
                Capital = capital,
                Population = population,
                ExpectedAnnualDeliveries = expectedDeliveries,
                DirectorName = "District Director of Health",
                Phone = "+233-000-000-000",
                Email = $"{code.ToLower()}@ghs.gov.gh",
                Latitude = 0,
                Longitude = 0,
                CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                Deleted = 0
            };
        }

        #endregion

        #region Update Facilities

        private async Task UpdateFacilitiesWithRegionDistrictAsync()
        {
            _logger.LogInformation("Updating existing facilities with region and district mappings...");

            // Known facility IDs from the mobile app
            var korleBuId = Guid.Parse("5f021d67-3ceb-44cd-8f55-5b10ca9039e1");
            var greaterAccraRegionId = Guid.Parse("11111111-1111-1111-1111-111111111101");
            var accraMetropolitanDistrictId = Guid.Parse("22222222-2222-2222-2222-222222220001");
            var korleKlotteyDistrictId = Guid.Parse("22222222-2222-2222-2222-222222220017");

            // Update Korle Bu Teaching Hospital
            var korleBu = await _context.Facilities.Include(d => d.District).FirstOrDefaultAsync(f => f.ID == korleBuId);
            if (korleBu != null)
            {
                korleBu.District.RegionID = greaterAccraRegionId;
                korleBu.DistrictID = accraMetropolitanDistrictId;
                korleBu.Name = "Accra Metropolitan";
                korleBu.Level = "Tertiary";
                korleBu.UpdatedTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _logger.LogInformation("Updated Korle Bu Teaching Hospital with region and district.");
            }

            // Update Ridge Hospital (by code)
            var ridgeHospital = await _context.Facilities.Include(d => d.District).FirstOrDefaultAsync(f => f.Code == "RH");
            if (ridgeHospital != null)
            {
                ridgeHospital.District.RegionID = greaterAccraRegionId;
                ridgeHospital.DistrictID = korleKlotteyDistrictId;
                ridgeHospital.Name = "Korle Klottey Municipal";
                ridgeHospital.Level = "Tertiary";
                ridgeHospital.UpdatedTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _logger.LogInformation("Updated Ridge Hospital with region and district.");
            }

            // Update 37 Military Hospital (by code)
            var militaryHospital = await _context.Facilities.Include(d => d.District).FirstOrDefaultAsync(f => f.Code == "37MH");
            if (militaryHospital != null)
            {
                militaryHospital.District.RegionID = greaterAccraRegionId;
                militaryHospital.DistrictID = accraMetropolitanDistrictId;
                militaryHospital.Name = "Accra Metropolitan";
                militaryHospital.Level = "Tertiary";
                militaryHospital.UpdatedTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _logger.LogInformation("Updated 37 Military Hospital with region and district.");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Facility region/district mappings updated.");
        }

        #endregion

        #region Monitoring Users

        private async Task SeedMonitoringUsersAsync()
        {
            if (await _context.MonitoringUsers.AnyAsync())
            {
                _logger.LogInformation("Monitoring users already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding monitoring users...");

            var users = GetDefaultMonitoringUsers();
            await _context.MonitoringUsers.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {users.Count} monitoring users.");
        }

        private List<MonitoringUser> GetDefaultMonitoringUsers()
        {
            var now = DateTime.UtcNow;
            var greaterAccraRegionId = Guid.Parse("11111111-1111-1111-1111-111111111101");
            var accraDistrictId = Guid.Parse("22222222-2222-2222-2222-222222220001");

            return new List<MonitoringUser>
            {
                // National Admin - Full access
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330001"),
                    Email = "admin@emperorsoftware.co",
                    FirstName = "System",
                    LastName = "Administrator",
                    PasswordHash = HashPassword("system.password"),
                    Role = "Admin",
                    AccessLevel = "National",
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330001"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330001"),
                    Deleted = 0
                },
                // National Manager
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330002"),
                    Email = "manager@emperorsoftware.co",
                    FirstName = "National",
                    LastName = "Manager",
                    PasswordHash = HashPassword("system.password"),
                    Role = "Manager",
                    AccessLevel = "National",
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330002"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330002"),
                    Deleted = 0
                },
                // Regional Admin - Greater Accra
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330003"),
                    Email = "regional@emperorsoftware.co",
                    FirstName = "Regional",
                    LastName = "Admin",
                    PasswordHash = HashPassword("system.password"),
                    Role = "Admin",
                    AccessLevel = "Regional",
                    RegionID = greaterAccraRegionId,
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330003"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330003"),
                    Deleted = 0
                },
                // District Admin - Accra Metropolitan
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330004"),
                    Email = "district@emperorsoftware.co",
                    FirstName = "District",
                    LastName = "Admin",
                    PasswordHash = HashPassword("District@123"),
                    Role = "Admin",
                    AccessLevel = "District",
                    RegionID = greaterAccraRegionId,
                    DistrictID = accraDistrictId,
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330004"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330004"),
                    Deleted = 0
                },
                // Analyst User
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330005"),
                    Email = "analyst@emperorsoftware.co",
                    FirstName = "Data",
                    LastName = "Analyst",
                    PasswordHash = HashPassword("Analyst@123"),
                    Role = "Analyst",
                    AccessLevel = "National",
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330005"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330005"),
                    Deleted = 0
                },
                // Viewer User
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330006"),
                    Email = "viewer@emperorsoftware.co",
                    FirstName = "Report",
                    LastName = "Viewer",
                    PasswordHash = HashPassword("Viewer@123"),
                    Role = "Viewer",
                    AccessLevel = "National",
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330006"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330006"),
                    Deleted = 0
                },
                // Test Admin for development
                new MonitoringUser
                {
                    ID = Guid.Parse("33333333-3333-3333-3333-333333330007"),
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User",
                    PasswordHash = HashPassword("Test@123"),
                    Role = "Admin",
                    AccessLevel = "National",
                    IsActive = true,
                    EmailConfirmed = true,
                    RequirePasswordChange = false,
                    CreatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    UpdatedTime = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333330007"),
                    UpdatedBy = Guid.Parse("33333333-3333-3333-3333-333333330007"),
                    Deleted = 0
                }
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        #endregion
    }
}
