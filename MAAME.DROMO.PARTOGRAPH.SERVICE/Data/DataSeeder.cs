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
                await SeedFacilitiesAsync();
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

        #region Facilities

        private async Task SeedFacilitiesAsync()
        {
            if (await _context.Facilities.AnyAsync())
            {
                _logger.LogInformation("Facilities already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Seeding facilities...");

            var facilities = GetGhanaFacilities();
            await _context.Facilities.AddRangeAsync(facilities);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {facilities.Count} facilities.");
        }

        private List<Facility> GetGhanaFacilities()
        {
            var facilities = new List<Facility>();

            // ============================================
            // GREATER ACCRA REGION (IDs: 0001-0030)
            // ============================================
            var greaterAccraRegionId = Guid.Parse("11111111-1111-1111-1111-111111111101");
            var accraMetroId = Guid.Parse("22222222-2222-2222-2222-222222220001");
            var temaMetroId = Guid.Parse("22222222-2222-2222-2222-222222220002");
            var gaEastId = Guid.Parse("22222222-2222-2222-2222-222222220003");
            var gaWestId = Guid.Parse("22222222-2222-2222-2222-222222220004");
            var gaSouthId = Guid.Parse("22222222-2222-2222-2222-222222220005");
            var gaCentralId = Guid.Parse("22222222-2222-2222-2222-222222220006");
            var gaNorthId = Guid.Parse("22222222-2222-2222-2222-222222220007");
            var adentaId = Guid.Parse("22222222-2222-2222-2222-222222220008");
            var laDadzekpoId = Guid.Parse("22222222-2222-2222-2222-222222220009");
            var ledzokukuId = Guid.Parse("22222222-2222-2222-2222-222222220010");
            var kponeKatamangsoId = Guid.Parse("22222222-2222-2222-2222-222222220011");
            var ashaimanId = Guid.Parse("22222222-2222-2222-2222-222222220013");
            var korleKlotteyId = Guid.Parse("22222222-2222-2222-2222-222222220017");
            var ablekumaWestId = Guid.Parse("22222222-2222-2222-2222-222222220018");
            var ablekumaCentralId = Guid.Parse("22222222-2222-2222-2222-222222220019");
            var ablekumaNorthId = Guid.Parse("22222222-2222-2222-2222-222222220020");
            var okaikweiNorthId = Guid.Parse("22222222-2222-2222-2222-222222220021");
            var ayawasoCentralId = Guid.Parse("22222222-2222-2222-2222-222222220022");
            var ayawasoEastId = Guid.Parse("22222222-2222-2222-2222-222222220023");
            var ayawasoNorthId = Guid.Parse("22222222-2222-2222-2222-222222220024");
            var ayawasoWestId = Guid.Parse("22222222-2222-2222-2222-222222220025");
            var weloId = Guid.Parse("22222222-2222-2222-2222-222222220026");
            var ningoId = Guid.Parse("22222222-2222-2222-2222-222222220027");

            // Accra Metropolitan & Korle Klottey Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440001", "Korle Bu Teaching Hospital", "KBTH", "Hospital", "Tertiary", accraMetroId, "Accra", 5.5364, -0.2275),
                CreateFacility("44444444-4444-4444-4444-444444440002", "Ridge Hospital", "RH", "Hospital", "Regional", accraMetroId, "Accra", 5.5569, -0.1958),
                CreateFacility("44444444-4444-4444-4444-444444440003", "37 Military Hospital", "37MH", "Hospital", "Tertiary", accraMetroId, "Accra", 5.5833, -0.1833),
                CreateFacility("44444444-4444-4444-4444-444444440004", "Police Hospital", "PH", "Hospital", "Specialized", accraMetroId, "Accra", 5.5600, -0.2000),
                CreateFacility("44444444-4444-4444-4444-444444440005", "La General Hospital", "LGH", "Hospital", "District", laDadzekpoId, "La", 5.5700, -0.1700),
                CreateFacility("44444444-4444-4444-4444-444444440006", "Mamprobi Polyclinic", "MPC", "Polyclinic", "District", accraMetroId, "Mamprobi", 5.5400, -0.2200),
                CreateFacility("44444444-4444-4444-4444-444444440007", "Ussher Polyclinic", "UPC", "Polyclinic", "District", accraMetroId, "Ussher", 5.5300, -0.2100),
                CreateFacility("44444444-4444-4444-4444-444444440008", "Princess Marie Louise Hospital", "PMLH", "Hospital", "District", accraMetroId, "Accra", 5.5450, -0.2050),
            });

            // Tema Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440009", "Tema General Hospital", "TEMGH", "Hospital", "Regional", temaMetroId, "Tema", 5.6689, -0.0167),
                CreateFacility("44444444-4444-4444-4444-444444440010", "Tema Polyclinic", "TEMP", "Polyclinic", "District", temaMetroId, "Tema", 5.6700, -0.0200),
                CreateFacility("44444444-4444-4444-4444-444444440011", "Community 1 Health Centre", "C1HC", "Health Centre", "Sub-district", temaMetroId, "Tema Com. 1", 5.6650, -0.0150),
                CreateFacility("44444444-4444-4444-4444-444444440012", "Community 9 CHPS", "C9CHPS", "CHPS", "Community", temaMetroId, "Tema Com. 9", 5.6600, -0.0100),
            });

            // Ga East Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440013", "Ga East Municipal Hospital", "GEMH", "Hospital", "District", gaEastId, "Abokobi", 5.7000, -0.1500),
                CreateFacility("44444444-4444-4444-4444-444444440014", "Abokobi Health Centre", "AboHC", "Health Centre", "Sub-district", gaEastId, "Abokobi", 5.7050, -0.1550),
                CreateFacility("44444444-4444-4444-4444-444444440015", "Dome Health Centre", "DomHC", "Health Centre", "Sub-district", gaEastId, "Dome", 5.6500, -0.2300),
                CreateFacility("44444444-4444-4444-4444-444444440016", "Taifa CHPS", "TaiCHPS", "CHPS", "Community", gaEastId, "Taifa", 5.6600, -0.2400),
            });

            // Ga West Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440017", "Amasaman Hospital", "AmaH", "Hospital", "District", gaWestId, "Amasaman", 5.7000, -0.3000),
                CreateFacility("44444444-4444-4444-4444-444444440018", "Pokuase Health Centre", "PokHC", "Health Centre", "Sub-district", gaWestId, "Pokuase", 5.7100, -0.2800),
                CreateFacility("44444444-4444-4444-4444-444444440019", "Ofankor Health Centre", "OfaHC", "Health Centre", "Sub-district", gaWestId, "Ofankor", 5.6800, -0.2600),
            });

            // Ledzokuku Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440020", "Ledzokuku Krowor Municipal Hospital", "LKMH", "Hospital", "District", ledzokukuId, "Teshie", 5.5833, -0.1000),
                CreateFacility("44444444-4444-4444-4444-444444440021", "Teshie Polyclinic", "TesP", "Polyclinic", "District", ledzokukuId, "Teshie", 5.5800, -0.1050),
                CreateFacility("44444444-4444-4444-4444-444444440022", "Nungua Health Centre", "NunHC", "Health Centre", "Sub-district", ledzokukuId, "Nungua", 5.5900, -0.0800),
            });

            // Ashaiman Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440023", "Ashaiman Polyclinic", "AshP", "Polyclinic", "District", ashaimanId, "Ashaiman", 5.6833, -0.0333),
                CreateFacility("44444444-4444-4444-4444-444444440024", "Tulaku Health Centre", "TulHC", "Health Centre", "Sub-district", ashaimanId, "Tulaku", 5.6900, -0.0400),
                CreateFacility("44444444-4444-4444-4444-444444440025", "Lebanon CHPS", "LebCHPS", "CHPS", "Community", ashaimanId, "Lebanon", 5.6800, -0.0300),
            });

            // Ga South Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440026", "Ga South Municipal Hospital", "GSMH", "Hospital", "District", gaSouthId, "Weija", 5.5500, -0.3500),
                CreateFacility("44444444-4444-4444-4444-444444440027", "Weija Health Centre", "WeiHC", "Health Centre", "Sub-district", gaSouthId, "Weija", 5.5550, -0.3550),
            });

            // Adentan Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440028", "Adentan Municipal Hospital", "AdMH", "Hospital", "District", adentaId, "Adentan", 5.7167, -0.1500),
                CreateFacility("44444444-4444-4444-4444-444444440029", "Frafraha CHPS", "FraCHPS", "CHPS", "Community", adentaId, "Frafraha", 5.7200, -0.1400),
            });

            // Kpone Katamanso District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440030", "Kpone Health Centre", "KpoHC", "Health Centre", "Sub-district", kponeKatamangsoId, "Kpone", 5.6833, 0.0500),
            });

            // ============================================
            // ASHANTI REGION (IDs: 0031-0060)
            // ============================================
            var ashantiRegionId = Guid.Parse("11111111-1111-1111-1111-111111111102");
            var kumasiMetroId = Guid.Parse("22222222-2222-2222-2222-222222220028");
            var oforikromId = Guid.Parse("22222222-2222-2222-2222-222222220029");
            var asokwaId = Guid.Parse("22222222-2222-2222-2222-222222220030");
            var suameId = Guid.Parse("22222222-2222-2222-2222-222222220031");
            var oldTafoId = Guid.Parse("22222222-2222-2222-2222-222222220032");
            var kwadasoId = Guid.Parse("22222222-2222-2222-2222-222222220033");
            var ejismuId = Guid.Parse("22222222-2222-2222-2222-222222220037");
            var mampongId = Guid.Parse("22222222-2222-2222-2222-222222220039");
            var obuasiId = Guid.Parse("22222222-2222-2222-2222-222222220057");
            var bekwaiId = Guid.Parse("22222222-2222-2222-2222-222222220052");
            var afigya_kwabreId = Guid.Parse("22222222-2222-2222-2222-222222220034");
            var atwima_nwabiaghyaId = Guid.Parse("22222222-2222-2222-2222-222222220040");
            var asanteAkimCentralId = Guid.Parse("22222222-2222-2222-2222-222222220045");

            // Kumasi Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440031", "Komfo Anokye Teaching Hospital", "KATH", "Hospital", "Tertiary", kumasiMetroId, "Kumasi", 6.6885, -1.6244),
                CreateFacility("44444444-4444-4444-4444-444444440032", "Kumasi South Hospital", "KSH", "Hospital", "Regional", kumasiMetroId, "Kumasi", 6.6700, -1.6300),
                CreateFacility("44444444-4444-4444-4444-444444440033", "Suntreso Government Hospital", "SGH", "Hospital", "District", kumasiMetroId, "Suntreso", 6.7000, -1.6400),
                CreateFacility("44444444-4444-4444-4444-444444440034", "Manhyia District Hospital", "ManDH", "Hospital", "District", kumasiMetroId, "Manhyia", 6.7100, -1.6100),
                CreateFacility("44444444-4444-4444-4444-444444440035", "Tafo Government Hospital", "TafoGH", "Hospital", "District", oldTafoId, "Tafo", 6.7200, -1.5900),
                CreateFacility("44444444-4444-4444-4444-444444440036", "Maternal and Child Health Hospital Kumasi", "MCHK", "Hospital", "Specialized", kumasiMetroId, "Kumasi", 6.6900, -1.6250),
            });

            // Oforikrom Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440037", "KNUST Hospital", "KNUSTH", "Hospital", "Tertiary", oforikromId, "KNUST", 6.6730, -1.5660),
                CreateFacility("44444444-4444-4444-4444-444444440038", "Oforikrom Health Centre", "OfoHC", "Health Centre", "Sub-district", oforikromId, "Oforikrom", 6.6800, -1.5700),
                CreateFacility("44444444-4444-4444-4444-444444440039", "Ayigya Health Centre", "AyiHC", "Health Centre", "Sub-district", oforikromId, "Ayigya", 6.6650, -1.5750),
                CreateFacility("44444444-4444-4444-4444-444444440040", "Bomso CHPS", "BomCHPS", "CHPS", "Community", oforikromId, "Bomso", 6.6700, -1.5650),
            });

            // Ejisu Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440041", "Ejisu Government Hospital", "EjGH", "Hospital", "District", ejismuId, "Ejisu", 6.7261, -1.4614),
                CreateFacility("44444444-4444-4444-4444-444444440042", "Besease Health Centre", "BesHC", "Health Centre", "Sub-district", ejismuId, "Besease", 6.7400, -1.4800),
                CreateFacility("44444444-4444-4444-4444-444444440043", "Bonwire Health Centre", "BonHC", "Health Centre", "Sub-district", ejismuId, "Bonwire", 6.7500, -1.4500),
                CreateFacility("44444444-4444-4444-4444-444444440044", "Kwaso CHPS", "KwaCHPS", "CHPS", "Community", ejismuId, "Kwaso", 6.7300, -1.4700),
            });

            // Mampong Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440045", "Mampong Government Hospital", "MamGH", "Hospital", "District", mampongId, "Mampong", 7.0619, -1.4011),
                CreateFacility("44444444-4444-4444-4444-444444440046", "Kofiase Health Centre", "KofHC", "Health Centre", "Sub-district", mampongId, "Kofiase", 7.0800, -1.4200),
                CreateFacility("44444444-4444-4444-4444-444444440047", "Nsuta Health Centre", "NsuHC", "Health Centre", "Sub-district", mampongId, "Nsuta", 7.0400, -1.3900),
                CreateFacility("44444444-4444-4444-4444-444444440048", "Benim CHPS", "BenCHPS", "CHPS", "Community", mampongId, "Benim", 7.0500, -1.4100),
            });

            // Obuasi Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440049", "AGA Hospital Obuasi", "AGAH", "Hospital", "Private", obuasiId, "Obuasi", 6.2028, -1.6628),
                CreateFacility("44444444-4444-4444-4444-444444440050", "Obuasi Government Hospital", "ObuGH", "Hospital", "District", obuasiId, "Obuasi", 6.2000, -1.6600),
                CreateFacility("44444444-4444-4444-4444-444444440051", "Tutuka Health Centre", "TutHC", "Health Centre", "Sub-district", obuasiId, "Tutuka", 6.2100, -1.6700),
                CreateFacility("44444444-4444-4444-4444-444444440052", "Wawase CHPS", "WawCHPS", "CHPS", "Community", obuasiId, "Wawase", 6.1900, -1.6500),
            });

            // Bekwai Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440053", "Bekwai Municipal Hospital", "BekMH", "Hospital", "District", bekwaiId, "Bekwai", 6.4556, -1.5717),
                CreateFacility("44444444-4444-4444-4444-444444440054", "Anwiankwanta Health Centre", "AnwHC", "Health Centre", "Sub-district", bekwaiId, "Anwiankwanta", 6.4700, -1.5800),
                CreateFacility("44444444-4444-4444-4444-444444440055", "Kokoben CHPS", "KokCHPS", "CHPS", "Community", bekwaiId, "Kokoben", 6.4400, -1.5600),
            });

            // Asante Akim Central Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440056", "Konongo Odumase Hospital", "KonOH", "Hospital", "District", asanteAkimCentralId, "Konongo", 6.6167, -1.2167),
                CreateFacility("44444444-4444-4444-4444-444444440057", "Agogo Presbyterian Hospital", "AgoPH", "Hospital", "District", asanteAkimCentralId, "Agogo", 6.8000, -1.0833),
            });

            // Afigya Kwabre South Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440058", "Afigya Kwabre South Hospital", "AKSH", "Hospital", "District", afigya_kwabreId, "Kodie", 6.8000, -1.6500),
                CreateFacility("44444444-4444-4444-4444-444444440059", "Maakro Health Centre", "MaaHC", "Health Centre", "Sub-district", afigya_kwabreId, "Maakro", 6.8100, -1.6600),
            });

            // Atwima Nwabiagya Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440060", "Nkawie Government Hospital", "NkaGH", "Hospital", "District", atwima_nwabiaghyaId, "Nkawie", 6.7500, -1.7833),
            });

            // ============================================
            // AHAFO REGION (IDs: 0061-0095)
            // ============================================
            var ahafoRegionId = Guid.Parse("11111111-1111-1111-1111-111111111115");
            var asunafoNorthId = Guid.Parse("22222222-2222-2222-2222-222222220236");
            var asunafoSouthId = Guid.Parse("22222222-2222-2222-2222-222222220237");
            var asutifiNorthId = Guid.Parse("22222222-2222-2222-2222-222222220238");
            var asutifiSouthId = Guid.Parse("22222222-2222-2222-2222-222222220239");
            var tanoNorthId = Guid.Parse("22222222-2222-2222-2222-222222220240");
            var tanoSouthId = Guid.Parse("22222222-2222-2222-2222-222222220241");

            // Asunafo North District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440061", "Goaso Government Hospital", "GoaGH", "Hospital", "District", asunafoNorthId, "Goaso", 6.8039, -2.5172),
                CreateFacility("44444444-4444-4444-4444-444444440062", "Mim Health Centre", "MimHC", "Health Centre", "Sub-district", asunafoNorthId, "Mim", 6.9500, -2.4667),
                CreateFacility("44444444-4444-4444-4444-444444440063", "Kasapin CHPS", "KasCHPS", "CHPS", "Community", asunafoNorthId, "Kasapin", 6.8200, -2.5000),
                CreateFacility("44444444-4444-4444-4444-444444440064", "Asafo-Akrodie CHPS", "AsaCHPS", "CHPS", "Community", asunafoNorthId, "Asafo-Akrodie", 6.8100, -2.4800),
                CreateFacility("44444444-4444-4444-4444-444444440065", "Ayomso CHPS", "AyoCHPS", "CHPS", "Community", asunafoNorthId, "Ayomso", 6.8300, -2.5300),
                CreateFacility("44444444-4444-4444-4444-444444440066", "Dwenase CHPS", "DweCHPS", "CHPS", "Community", asunafoNorthId, "Dwenase", 6.7900, -2.4900),
                CreateFacility("44444444-4444-4444-4444-444444440067", "Goaso Prisons Clinic", "GoaPC", "Clinic", "Sub-district", asunafoNorthId, "Goaso", 6.8050, -2.5180),
                CreateFacility("44444444-4444-4444-4444-444444440068", "Ntotroso Health Centre", "NtoHC", "Health Centre", "Sub-district", asunafoNorthId, "Ntotroso", 6.8800, -2.5500),
            });

            // Asunafo South District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440069", "Kukuom Health Centre", "KukHC", "Health Centre", "Sub-district", asunafoSouthId, "Kukuom", 6.6000, -2.5000),
                CreateFacility("44444444-4444-4444-4444-444444440070", "Abuom CHPS", "AbuCHPS", "CHPS", "Community", asunafoSouthId, "Abuom", 6.6200, -2.5200),
                CreateFacility("44444444-4444-4444-4444-444444440071", "Asumura CHPS", "AsuCHPS", "CHPS", "Community", asunafoSouthId, "Asumura", 6.5800, -2.4800),
                CreateFacility("44444444-4444-4444-4444-444444440072", "Dwomor CHPS", "DwoCHPS", "CHPS", "Community", asunafoSouthId, "Dwomor", 6.5500, -2.4600),
                CreateFacility("44444444-4444-4444-4444-444444440073", "Gyasikrom CHPS", "GyaCHPS", "CHPS", "Community", asunafoSouthId, "Gyasikrom", 6.6100, -2.4900),
            });

            // Asutifi North District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440074", "Kenyasi No. 1 Health Centre", "Ken1HC", "Health Centre", "Sub-district", asutifiNorthId, "Kenyasi No. 1", 6.9167, -2.3833),
                CreateFacility("44444444-4444-4444-4444-444444440075", "Kenyasi No. 2 Health Centre", "Ken2HC", "Health Centre", "Sub-district", asutifiNorthId, "Kenyasi No. 2", 6.9200, -2.3900),
                CreateFacility("44444444-4444-4444-4444-444444440076", "Gambia No. 1 CHPS", "Gam1CHPS", "CHPS", "Community", asutifiNorthId, "Gambia No. 1", 6.9300, -2.4000),
                CreateFacility("44444444-4444-4444-4444-444444440077", "Gambia No. 2 CHPS", "Gam2CHPS", "CHPS", "Community", asutifiNorthId, "Gambia No. 2", 6.9350, -2.4050),
                CreateFacility("44444444-4444-4444-4444-444444440078", "Wamahinso CHPS", "WamCHPS", "CHPS", "Community", asutifiNorthId, "Wamahinso", 6.9000, -2.3700),
            });

            // Asutifi South District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440079", "Hwidiem Government Hospital", "HwiGH", "Hospital", "District", asutifiSouthId, "Hwidiem", 6.7667, -2.3000),
                CreateFacility("44444444-4444-4444-4444-444444440080", "Atronie CHPS", "AtrCHPS", "CHPS", "Community", asutifiSouthId, "Atronie", 6.7800, -2.3200),
                CreateFacility("44444444-4444-4444-4444-444444440081", "Dadiesoaba CHPS", "DadCHPS", "CHPS", "Community", asutifiSouthId, "Dadiesoaba", 6.7500, -2.2800),
                CreateFacility("44444444-4444-4444-4444-444444440082", "Mehame CHPS", "MehCHPS", "CHPS", "Community", asutifiSouthId, "Mehame", 6.7600, -2.2900),
            });

            // Tano North District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440083", "Duayaw Nkwanta Government Hospital", "DuaGH", "Hospital", "District", tanoNorthId, "Duayaw Nkwanta", 7.1833, -2.1000),
                CreateFacility("44444444-4444-4444-4444-444444440084", "Yamfo Health Centre", "YamHC", "Health Centre", "Sub-district", tanoNorthId, "Yamfo", 7.2000, -2.0800),
                CreateFacility("44444444-4444-4444-4444-444444440085", "Bomaa CHPS", "BomaCHPS", "CHPS", "Community", tanoNorthId, "Bomaa", 7.1700, -2.1200),
                CreateFacility("44444444-4444-4444-4444-444444440086", "Tanoso Health Centre", "TanHC", "Health Centre", "Sub-district", tanoNorthId, "Tanoso", 7.1500, -2.0900),
                CreateFacility("44444444-4444-4444-4444-444444440087", "Adrobaa CHPS", "AdrCHPS", "CHPS", "Community", tanoNorthId, "Adrobaa", 7.1900, -2.1100),
            });

            // Tano South District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440088", "Bechem Government Hospital", "BecGH", "Hospital", "District", tanoSouthId, "Bechem", 7.0833, -2.0167),
                CreateFacility("44444444-4444-4444-4444-444444440089", "Techire CHPS", "TecCHPS", "CHPS", "Community", tanoSouthId, "Techire", 7.0600, -2.0300),
                CreateFacility("44444444-4444-4444-4444-444444440090", "Nkaseim CHPS", "NkaCHPS", "CHPS", "Community", tanoSouthId, "Nkaseim", 7.0700, -2.0400),
                CreateFacility("44444444-4444-4444-4444-444444440091", "Aworowa CHPS", "AwoCHPS", "CHPS", "Community", tanoSouthId, "Aworowa", 7.0500, -2.0100),
                CreateFacility("44444444-4444-4444-4444-444444440092", "Derma CHPS", "DerCHPS", "CHPS", "Community", tanoSouthId, "Derma", 7.0900, -2.0200),
            });

            // ============================================
            // BONO REGION (IDs: 0093-0110)
            // ============================================
            var bonoRegionId = Guid.Parse("11111111-1111-1111-1111-111111111113");
            var sunyaniId = Guid.Parse("22222222-2222-2222-2222-222222220213");
            var sunyaniWestId = Guid.Parse("22222222-2222-2222-2222-222222220214");
            var berekumEastId = Guid.Parse("22222222-2222-2222-2222-222222220215");
            var dormaaId = Guid.Parse("22222222-2222-2222-2222-222222220217");
            var wenchiId = Guid.Parse("22222222-2222-2222-2222-222222220223");

            // Sunyani Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440093", "Sunyani Regional Hospital", "SunRH", "Hospital", "Regional", sunyaniId, "Sunyani", 7.3349, -2.3123),
                CreateFacility("44444444-4444-4444-4444-444444440094", "Sunyani Municipal Hospital", "SunMH", "Hospital", "District", sunyaniId, "Sunyani", 7.3400, -2.3100),
                CreateFacility("44444444-4444-4444-4444-444444440095", "New Dormaa Health Centre", "NDHC", "Health Centre", "Sub-district", sunyaniId, "New Dormaa", 7.3500, -2.3200),
                CreateFacility("44444444-4444-4444-4444-444444440096", "Abesim Health Centre", "AbeHC", "Health Centre", "Sub-district", sunyaniId, "Abesim", 7.3200, -2.3000),
            });

            // Berekum East Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440097", "Holy Family Hospital Berekum", "HFHB", "Hospital", "District", berekumEastId, "Berekum", 7.4567, -2.5850),
                CreateFacility("44444444-4444-4444-4444-444444440098", "Berekum Government Hospital", "BerGH", "Hospital", "District", berekumEastId, "Berekum", 7.4600, -2.5800),
                CreateFacility("44444444-4444-4444-4444-444444440099", "Koraso Health Centre", "KorHC", "Health Centre", "Sub-district", berekumEastId, "Koraso", 7.4700, -2.5900),
            });

            // Dormaa Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440100", "Dormaa Ahenkro Presbyterian Hospital", "DAPH", "Hospital", "District", dormaaId, "Dormaa Ahenkro", 7.3500, -2.9667),
                CreateFacility("44444444-4444-4444-4444-444444440101", "Dormaa Government Hospital", "DorGH", "Hospital", "District", dormaaId, "Dormaa", 7.3550, -2.9700),
                CreateFacility("44444444-4444-4444-4444-444444440102", "Kwameasua Health Centre", "KwamHC", "Health Centre", "Sub-district", dormaaId, "Kwameasua", 7.3600, -2.9800),
            });

            // Wenchi Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440103", "Wenchi Methodist Hospital", "WenMH", "Hospital", "District", wenchiId, "Wenchi", 7.7500, -2.1000),
                CreateFacility("44444444-4444-4444-4444-444444440104", "Wenchi Government Hospital", "WenGH", "Hospital", "District", wenchiId, "Wenchi", 7.7550, -2.0950),
                CreateFacility("44444444-4444-4444-4444-444444440105", "Subinso Health Centre", "SubHC", "Health Centre", "Sub-district", wenchiId, "Subinso", 7.7600, -2.1100),
            });

            // ============================================
            // BONO EAST REGION (IDs: 0106-0120)
            // ============================================
            var bonoEastRegionId = Guid.Parse("11111111-1111-1111-1111-111111111114");
            var techimanId = Guid.Parse("22222222-2222-2222-2222-222222220225");
            var nkoranzaSouthId = Guid.Parse("22222222-2222-2222-2222-222222220228");
            var kintampoNorthId = Guid.Parse("22222222-2222-2222-2222-222222220229");
            var atebubuId = Guid.Parse("22222222-2222-2222-2222-222222220231");

            // Techiman Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440106", "Holy Family Hospital Techiman", "HFHT", "Hospital", "Regional", techimanId, "Techiman", 7.5833, -1.9333),
                CreateFacility("44444444-4444-4444-4444-444444440107", "Techiman Municipal Hospital", "TMH", "Hospital", "District", techimanId, "Techiman", 7.5800, -1.9300),
                CreateFacility("44444444-4444-4444-4444-444444440108", "Krobo Health Centre", "KroHC", "Health Centre", "Sub-district", techimanId, "Krobo", 7.5900, -1.9400),
                CreateFacility("44444444-4444-4444-4444-444444440109", "Aworowa CHPS Techiman", "AwTCHPS", "CHPS", "Community", techimanId, "Aworowa", 7.5700, -1.9200),
            });

            // Kintampo North Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440110", "Kintampo Municipal Hospital", "KMH", "Hospital", "District", kintampoNorthId, "Kintampo", 8.0556, -1.7306),
                CreateFacility("44444444-4444-4444-4444-444444440111", "Kintampo Health Research Centre", "KHRC", "Research Centre", "Specialized", kintampoNorthId, "Kintampo", 8.0600, -1.7350),
                CreateFacility("44444444-4444-4444-4444-444444440112", "Babato Health Centre", "BabHC", "Health Centre", "Sub-district", kintampoNorthId, "Babato", 8.0700, -1.7400),
                CreateFacility("44444444-4444-4444-4444-444444440113", "New Longoro CHPS", "NLCHPS", "CHPS", "Community", kintampoNorthId, "New Longoro", 8.0400, -1.7200),
            });

            // Atebubu Amantin Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440114", "Atebubu Government Hospital", "AtGH", "Hospital", "District", atebubuId, "Atebubu", 7.7500, -0.9833),
                CreateFacility("44444444-4444-4444-4444-444444440115", "Amantin Health Centre", "AmHC", "Health Centre", "Sub-district", atebubuId, "Amantin", 7.7600, -0.9900),
                CreateFacility("44444444-4444-4444-4444-444444440116", "Fakwasi CHPS", "FakCHPS", "CHPS", "Community", atebubuId, "Fakwasi", 7.7400, -0.9700),
            });

            // Central Region Facilities
            var centralRegionId = Guid.Parse("11111111-1111-1111-1111-111111111104");
            var capeCoastId = Guid.Parse("22222222-2222-2222-2222-222222220086");
            var effutuId = Guid.Parse("22222222-2222-2222-2222-222222220095");
            var awutuSenyaEastId = Guid.Parse("22222222-2222-2222-2222-222222220097");
            var agonaWestId = Guid.Parse("22222222-2222-2222-2222-222222220099");
            var assinNorthId = Guid.Parse("22222222-2222-2222-2222-222222220101");

            // Cape Coast Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440117", "Cape Coast Teaching Hospital", "CCTH", "Hospital", "Tertiary", capeCoastId, "Cape Coast", 5.1315, -1.2795),
                CreateFacility("44444444-4444-4444-4444-444444440118", "University of Cape Coast Hospital", "UCCH", "Hospital", "Tertiary", capeCoastId, "UCC", 5.1150, -1.2900),
                CreateFacility("44444444-4444-4444-4444-444444440119", "Ewim Polyclinic", "EP", "Polyclinic", "District", capeCoastId, "Ewim", 5.1200, -1.2700),
                CreateFacility("44444444-4444-4444-4444-444444440120", "Aboom Health Centre", "AbHC2", "Health Centre", "Sub-district", capeCoastId, "Aboom", 5.1100, -1.2600),
            });

            // Effutu Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440121", "Winneba Municipal Hospital", "WinMH", "Hospital", "District", effutuId, "Winneba", 5.3500, -0.6333),
                CreateFacility("44444444-4444-4444-4444-444444440122", "University of Education Winneba Clinic", "UEWC", "Clinic", "Sub-district", effutuId, "Winneba", 5.3600, -0.6400),
                CreateFacility("44444444-4444-4444-4444-444444440123", "Sankor Health Centre", "SanHC", "Health Centre", "Sub-district", effutuId, "Sankor", 5.3400, -0.6200),
            });

            // Awutu Senya East Municipal (Kasoa) Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440124", "Kasoa Polyclinic", "KasP", "Polyclinic", "District", awutuSenyaEastId, "Kasoa", 5.5333, -0.4167),
                CreateFacility("44444444-4444-4444-4444-444444440125", "Budumburam Health Centre", "BudHC", "Health Centre", "Sub-district", awutuSenyaEastId, "Budumburam", 5.5400, -0.4300),
                CreateFacility("44444444-4444-4444-4444-444444440126", "Ofankor Health Centre", "OfaHC", "Health Centre", "Sub-district", awutuSenyaEastId, "Ofankor", 5.5200, -0.4100),
                CreateFacility("44444444-4444-4444-4444-444444440127", "Opeikuma CHPS", "OpeCHPS", "CHPS", "Community", awutuSenyaEastId, "Opeikuma", 5.5500, -0.4200),
            });

            // Agona West Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440128", "Agona Swedru Municipal Hospital", "ASMH", "Hospital", "District", agonaWestId, "Swedru", 5.5333, -0.7000),
                CreateFacility("44444444-4444-4444-4444-444444440129", "Abodom Health Centre", "AboHC", "Health Centre", "Sub-district", agonaWestId, "Abodom", 5.5400, -0.7100),
                CreateFacility("44444444-4444-4444-4444-444444440130", "Nyakrom Health Centre", "NyaHC", "Health Centre", "Sub-district", agonaWestId, "Nyakrom", 5.5200, -0.6900),
            });

            // Assin North Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440131", "Assin Foso Municipal Hospital", "AFMH", "Hospital", "District", assinNorthId, "Assin Foso", 5.7167, -1.2167),
                CreateFacility("44444444-4444-4444-4444-444444440132", "Assin Praso Health Centre", "APHC", "Health Centre", "Sub-district", assinNorthId, "Assin Praso", 5.7300, -1.2300),
                CreateFacility("44444444-4444-4444-4444-444444440133", "Assin Bereku CHPS", "ABCHPS2", "CHPS", "Community", assinNorthId, "Assin Bereku", 5.7000, -1.2000),
            });

            // Eastern Region Facilities
            var easternRegionId = Guid.Parse("11111111-1111-1111-1111-111111111105");
            var newJuabenSouthId = Guid.Parse("22222222-2222-2222-2222-222222220107");
            var nsawamId = Guid.Parse("22222222-2222-2222-2222-222222220109");
            var eastAkimId = Guid.Parse("22222222-2222-2222-2222-222222220113");
            var birimCentralId = Guid.Parse("22222222-2222-2222-2222-222222220116");
            var kwahuWestId = Guid.Parse("22222222-2222-2222-2222-222222220120");

            // New Juaben South Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440134", "Eastern Regional Hospital", "ERH", "Hospital", "Regional", newJuabenSouthId, "Koforidua", 6.0940, -0.2577),
                CreateFacility("44444444-4444-4444-4444-444444440135", "St. Joseph's Hospital Koforidua", "SJH", "Hospital", "District", newJuabenSouthId, "Koforidua", 6.0900, -0.2600),
                CreateFacility("44444444-4444-4444-4444-444444440136", "Koforidua Polyclinic", "KP", "Polyclinic", "District", newJuabenSouthId, "Koforidua", 6.0950, -0.2550),
                CreateFacility("44444444-4444-4444-4444-444444440137", "Ada Health Centre", "AdaHC2", "Health Centre", "Sub-district", newJuabenSouthId, "Ada", 6.1000, -0.2700),
            });

            // Nsawam Adoagyiri Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440138", "Nsawam Government Hospital", "NGH", "Hospital", "District", nsawamId, "Nsawam", 5.8167, -0.3500),
                CreateFacility("44444444-4444-4444-4444-444444440139", "Adoagyiri Health Centre", "AdoHC", "Health Centre", "Sub-district", nsawamId, "Adoagyiri", 5.8200, -0.3600),
                CreateFacility("44444444-4444-4444-4444-444444440140", "Dobro CHPS", "DobCHPS", "CHPS", "Community", nsawamId, "Dobro", 5.8100, -0.3400),
            });

            // East Akim Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440141", "Kibi Government Hospital", "KiGH", "Hospital", "District", eastAkimId, "Kibi", 6.1667, -0.5500),
                CreateFacility("44444444-4444-4444-4444-444444440142", "Bunso Health Centre", "BunHC", "Health Centre", "Sub-district", eastAkimId, "Bunso", 6.1800, -0.5600),
                CreateFacility("44444444-4444-4444-4444-444444440143", "Apedwa CHPS", "ApeCHPS", "CHPS", "Community", eastAkimId, "Apedwa", 6.1500, -0.5400),
            });

            // Birim Central Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440144", "Oda Government Hospital", "OdaGH", "Hospital", "District", birimCentralId, "Oda", 5.9167, -0.9833),
                CreateFacility("44444444-4444-4444-4444-444444440145", "Akim Oda Polyclinic", "AODP", "Polyclinic", "District", birimCentralId, "Akim Oda", 5.9200, -0.9800),
                CreateFacility("44444444-4444-4444-4444-444444440146", "Akroso Health Centre", "AkrHC", "Health Centre", "Sub-district", birimCentralId, "Akroso", 5.9300, -0.9900),
            });

            // Greater Accra Region Facilities
            var greaterAccraRegionId = Guid.Parse("11111111-1111-1111-1111-111111111101");
            var accraMetroId = Guid.Parse("22222222-2222-2222-2222-222222220001");
            var temaMetroId = Guid.Parse("22222222-2222-2222-2222-222222220002");
            var gaEastId = Guid.Parse("22222222-2222-2222-2222-222222220003");
            var gaWestId = Guid.Parse("22222222-2222-2222-2222-222222220004");
            var gaSouthId = Guid.Parse("22222222-2222-2222-2222-222222220005");
            var korleKlotteyId = Guid.Parse("22222222-2222-2222-2222-222222220017");
            var ledzokukuId = Guid.Parse("22222222-2222-2222-2222-222222220010");
            var ashaimanId = Guid.Parse("22222222-2222-2222-2222-222222220013");

            // Accra Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440001", "Korle Bu Teaching Hospital", "KBTH", "Hospital", "Tertiary", accraMetroId, "Accra", 5.5364, -0.2275),
                CreateFacility("44444444-4444-4444-4444-444444440002", "Ridge Hospital", "RH", "Hospital", "Regional", accraMetroId, "Accra", 5.5569, -0.1958),
                CreateFacility("44444444-4444-4444-4444-444444440003", "37 Military Hospital", "37MH", "Hospital", "Tertiary", accraMetroId, "Accra", 5.5833, -0.1833),
                CreateFacility("44444444-4444-4444-4444-444444440004", "Police Hospital", "PH", "Hospital", "Specialized", accraMetroId, "Accra", 5.5600, -0.2000),
                CreateFacility("44444444-4444-4444-4444-444444440005", "La General Hospital", "LGH", "Hospital", "District", accraMetroId, "La", 5.5700, -0.1700),
                CreateFacility("44444444-4444-4444-4444-444444440006", "Mamprobi Polyclinic", "MP", "Polyclinic", "District", accraMetroId, "Mamprobi", 5.5400, -0.2200),
                CreateFacility("44444444-4444-4444-4444-444444440007", "Ussher Polyclinic", "UP", "Polyclinic", "District", accraMetroId, "Ussher", 5.5300, -0.2100),
            });

            // Tema Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440008", "Tema General Hospital", "TEMGH", "Hospital", "Regional", temaMetroId, "Tema", 5.6689, -0.0167),
                CreateFacility("44444444-4444-4444-4444-444444440009", "Tema Polyclinic", "TEMP", "Polyclinic", "District", temaMetroId, "Tema", 5.6700, -0.0200),
                CreateFacility("44444444-4444-4444-4444-444444440010", "Community 1 Health Centre", "C1HC", "Health Centre", "Sub-district", temaMetroId, "Tema Com. 1", 5.6650, -0.0150),
                CreateFacility("44444444-4444-4444-4444-444444440011", "Community 9 CHPS", "C9CHPS", "CHPS", "Community", temaMetroId, "Tema Com. 9", 5.6600, -0.0100),
            });

            // Ga East Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440012", "Ga East Municipal Hospital", "GEMH", "Hospital", "District", gaEastId, "Abokobi", 5.7000, -0.1500),
                CreateFacility("44444444-4444-4444-4444-444444440013", "Abokobi Health Centre", "AboHC3", "Health Centre", "Sub-district", gaEastId, "Abokobi", 5.7050, -0.1550),
                CreateFacility("44444444-4444-4444-4444-444444440014", "Dome Health Centre", "DomHC", "Health Centre", "Sub-district", gaEastId, "Dome", 5.6500, -0.2300),
                CreateFacility("44444444-4444-4444-4444-444444440015", "Taifa CHPS", "TaiCHPS", "CHPS", "Community", gaEastId, "Taifa", 5.6600, -0.2400),
            });

            // Ga West Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440016", "Amasaman Hospital", "AmaH", "Hospital", "District", gaWestId, "Amasaman", 5.7000, -0.3000),
                CreateFacility("44444444-4444-4444-4444-444444440017", "Pokuase Health Centre", "PokHC", "Health Centre", "Sub-district", gaWestId, "Pokuase", 5.7100, -0.2800),
                CreateFacility("44444444-4444-4444-4444-444444440018", "Ofankor Health Centre GA", "OfaHC2", "Health Centre", "Sub-district", gaWestId, "Ofankor", 5.6800, -0.2600),
            });

            // Ledzokuku Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440019", "Ledzokuku Krowor Municipal Hospital", "LKMH", "Hospital", "District", ledzokukuId, "Teshie", 5.5833, -0.1000),
                CreateFacility("44444444-4444-4444-4444-444444440020", "Teshie Polyclinic", "TesP", "Polyclinic", "District", ledzokukuId, "Teshie", 5.5800, -0.1050),
                CreateFacility("44444444-4444-4444-4444-444444440021", "Nungua Health Centre", "NunHC", "Health Centre", "Sub-district", ledzokukuId, "Nungua", 5.5900, -0.0800),
            });

            // Ashaiman Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440022", "Ashaiman Polyclinic", "AshP", "Polyclinic", "District", ashaimanId, "Ashaiman", 5.6833, -0.0333),
                CreateFacility("44444444-4444-4444-4444-444444440023", "Tulaku Health Centre", "TulHC", "Health Centre", "Sub-district", ashaimanId, "Tulaku", 5.6900, -0.0400),
                CreateFacility("44444444-4444-4444-4444-444444440024", "Lebanon CHPS", "LebCHPS", "CHPS", "Community", ashaimanId, "Lebanon", 5.6800, -0.0300),
            });

            // Northern Region Facilities
            var northernRegionId = Guid.Parse("11111111-1111-1111-1111-111111111108");
            var tamaleMetroId = Guid.Parse("22222222-2222-2222-2222-222222220159");
            var sagnariguId = Guid.Parse("22222222-2222-2222-2222-222222220160");
            var saveluguid = Guid.Parse("22222222-2222-2222-2222-222222220163");
            var yendiId = Guid.Parse("22222222-2222-2222-2222-222222220166");

            // Tamale Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440147", "Tamale Teaching Hospital", "TTH", "Hospital", "Tertiary", tamaleMetroId, "Tamale", 9.4008, -0.8393),
                CreateFacility("44444444-4444-4444-4444-444444440148", "Tamale Central Hospital", "TCH", "Hospital", "Regional", tamaleMetroId, "Tamale", 9.4050, -0.8400),
                CreateFacility("44444444-4444-4444-4444-444444440149", "Tamale West Hospital", "TWH", "Hospital", "District", tamaleMetroId, "Tamale West", 9.4100, -0.8500),
                CreateFacility("44444444-4444-4444-4444-444444440150", "Vittin Health Centre", "VitHC", "Health Centre", "Sub-district", tamaleMetroId, "Vittin", 9.3900, -0.8300),
            });

            // Sagnarigu Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440151", "Sagnarigu District Hospital", "SagDH", "Hospital", "District", sagnariguId, "Sagnarigu", 9.4500, -0.8600),
                CreateFacility("44444444-4444-4444-4444-444444440152", "Choggu Health Centre", "ChoHC", "Health Centre", "Sub-district", sagnariguId, "Choggu", 9.4600, -0.8700),
                CreateFacility("44444444-4444-4444-4444-444444440153", "Gurugu CHPS", "GurCHPS", "CHPS", "Community", sagnariguId, "Gurugu", 9.4400, -0.8500),
            });

            // Savelugu Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440154", "Savelugu Municipal Hospital", "SavMH", "Hospital", "District", saveluguid, "Savelugu", 9.6242, -0.8250),
                CreateFacility("44444444-4444-4444-4444-444444440155", "Pong Tamale Health Centre", "PTHC", "Health Centre", "Sub-district", saveluguid, "Pong Tamale", 9.6300, -0.8300),
                CreateFacility("44444444-4444-4444-4444-444444440156", "Diare CHPS", "DiaCHPS", "CHPS", "Community", saveluguid, "Diare", 9.6100, -0.8200),
            });

            // Yendi Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440157", "Yendi Municipal Hospital", "YMH", "Hospital", "District", yendiId, "Yendi", 9.4425, -0.0097),
                CreateFacility("44444444-4444-4444-4444-444444440158", "Gnani Health Centre", "GnaHC", "Health Centre", "Sub-district", yendiId, "Gnani", 9.4500, -0.0200),
                CreateFacility("44444444-4444-4444-4444-444444440159", "Adibo CHPS", "AdibCHPS", "CHPS", "Community", yendiId, "Adibo", 9.4300, 0.0000),
            });

            // Upper East Region Facilities
            var upperEastRegionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var bolgatangaId = Guid.Parse("22222222-2222-2222-2222-222222220187");
            var bawkuId = Guid.Parse("22222222-2222-2222-2222-222222220193");
            var navrongoId = Guid.Parse("22222222-2222-2222-2222-222222220201");

            // Bolgatanga Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440160", "Bolgatanga Regional Hospital", "BRH", "Hospital", "Regional", bolgatangaId, "Bolgatanga", 10.7856, -0.8519),
                CreateFacility("44444444-4444-4444-4444-444444440161", "Upper East Regional Hospital", "UERH", "Hospital", "Regional", bolgatangaId, "Bolgatanga", 10.7900, -0.8550),
                CreateFacility("44444444-4444-4444-4444-444444440162", "Zuarungu Health Centre", "ZuaHC", "Health Centre", "Sub-district", bolgatangaId, "Zuarungu", 10.7800, -0.8400),
            });

            // Bawku Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440163", "Bawku Presbyterian Hospital", "BPH", "Hospital", "District", bawkuId, "Bawku", 11.0606, -0.2417),
                CreateFacility("44444444-4444-4444-4444-444444440164", "Bawku Health Centre", "BawHC", "Health Centre", "Sub-district", bawkuId, "Bawku", 11.0650, -0.2450),
                CreateFacility("44444444-4444-4444-4444-444444440165", "Gozesi CHPS", "GozCHPS", "CHPS", "Community", bawkuId, "Gozesi", 11.0500, -0.2300),
            });

            // Kassena Nankana Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440166", "War Memorial Hospital Navrongo", "WMHN", "Hospital", "District", navrongoId, "Navrongo", 10.8944, -1.0917),
                CreateFacility("44444444-4444-4444-4444-444444440167", "Navrongo Health Research Centre", "NHRC", "Research Centre", "Specialized", navrongoId, "Navrongo", 10.8900, -1.0950),
                CreateFacility("44444444-4444-4444-4444-444444440168", "Paga Health Centre", "PagHC", "Health Centre", "Sub-district", navrongoId, "Paga", 10.9800, -1.1100),
            });

            // Upper West Region Facilities
            var upperWestRegionId = Guid.Parse("11111111-1111-1111-1111-111111111112");
            var waId = Guid.Parse("22222222-2222-2222-2222-222222220202");
            var jirapaId = Guid.Parse("22222222-2222-2222-2222-222222220207");
            var lawraId = Guid.Parse("22222222-2222-2222-2222-222222220209");

            // Wa Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440169", "Wa Regional Hospital", "WRH", "Hospital", "Regional", waId, "Wa", 10.0601, -2.5099),
                CreateFacility("44444444-4444-4444-4444-444444440170", "Wa Municipal Hospital", "WaMH", "Hospital", "District", waId, "Wa", 10.0650, -2.5050),
                CreateFacility("44444444-4444-4444-4444-444444440171", "Kpongu Health Centre", "KpoHC", "Health Centre", "Sub-district", waId, "Kpongu", 10.0700, -2.5200),
                CreateFacility("44444444-4444-4444-4444-444444440172", "Bamahu CHPS", "BamCHPS", "CHPS", "Community", waId, "Bamahu", 10.0500, -2.5000),
            });

            // Jirapa Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440173", "St. Joseph Hospital Jirapa", "SJHJ", "Hospital", "District", jirapaId, "Jirapa", 10.7833, -2.5500),
                CreateFacility("44444444-4444-4444-4444-444444440174", "Hain Health Centre", "HaiHC", "Health Centre", "Sub-district", jirapaId, "Hain", 10.7900, -2.5600),
                CreateFacility("44444444-4444-4444-4444-444444440175", "Ullo CHPS", "UllCHPS", "CHPS", "Community", jirapaId, "Ullo", 10.7700, -2.5400),
            });

            // Lawra Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440176", "Lawra District Hospital", "LDH", "Hospital", "District", lawraId, "Lawra", 10.6333, -2.9000),
                CreateFacility("44444444-4444-4444-4444-444444440177", "Babile Health Centre", "BabHC2", "Health Centre", "Sub-district", lawraId, "Babile", 10.6400, -2.9100),
                CreateFacility("44444444-4444-4444-4444-444444440178", "Eremon CHPS", "EreCHPS", "CHPS", "Community", lawraId, "Eremon", 10.6200, -2.8900),
            });

            // Volta Region Facilities
            var voltaRegionId = Guid.Parse("11111111-1111-1111-1111-111111111106");
            var hoId = Guid.Parse("22222222-2222-2222-2222-222222220135");
            var ketaSouthId = Guid.Parse("22222222-2222-2222-2222-222222220141");
            var ketaId = Guid.Parse("22222222-2222-2222-2222-222222220143");

            // Ho Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440179", "Ho Teaching Hospital", "HTH", "Hospital", "Tertiary", hoId, "Ho", 6.6000, 0.4700),
                CreateFacility("44444444-4444-4444-4444-444444440180", "Volta Regional Hospital", "VRH", "Hospital", "Regional", hoId, "Ho", 6.6050, 0.4750),
                CreateFacility("44444444-4444-4444-4444-444444440181", "Ho Municipal Hospital", "HMH", "Hospital", "District", hoId, "Ho", 6.6100, 0.4650),
                CreateFacility("44444444-4444-4444-4444-444444440182", "Sokode Health Centre", "SokHC", "Health Centre", "Sub-district", hoId, "Sokode", 6.5900, 0.4600),
            });

            // Ketu South Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440183", "Ketu South Municipal Hospital", "KSMH", "Hospital", "District", ketaSouthId, "Denu", 6.0833, 1.1333),
                CreateFacility("44444444-4444-4444-4444-444444440184", "Aflao Health Centre", "AflHC", "Health Centre", "Sub-district", ketaSouthId, "Aflao", 6.1167, 1.1833),
                CreateFacility("44444444-4444-4444-4444-444444440185", "Agbozume CHPS", "AgbCHPS", "CHPS", "Community", ketaSouthId, "Agbozume", 6.1000, 1.1500),
            });

            // Keta Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440186", "Keta Municipal Hospital", "KetMH", "Hospital", "District", ketaId, "Keta", 5.9167, 0.9833),
                CreateFacility("44444444-4444-4444-4444-444444440187", "Anloga Health Centre", "AnlHC", "Health Centre", "Sub-district", ketaId, "Anloga", 5.7833, 0.8833),
                CreateFacility("44444444-4444-4444-4444-444444440188", "Woe CHPS", "WoeCHPS", "CHPS", "Community", ketaId, "Woe", 5.9000, 0.9700),
            });

            // Western Region Facilities
            var westernRegionId = Guid.Parse("11111111-1111-1111-1111-111111111103");
            var sekondiTakoradiId = Guid.Parse("22222222-2222-2222-2222-222222220071");
            var tarkwaId = Guid.Parse("22222222-2222-2222-2222-222222220078");
            var axim = Guid.Parse("22222222-2222-2222-2222-222222220085");

            // Sekondi Takoradi Metropolitan Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440189", "Effia Nkwanta Regional Hospital", "ENRH", "Hospital", "Regional", sekondiTakoradiId, "Sekondi", 4.9400, -1.7700),
                CreateFacility("44444444-4444-4444-4444-444444440190", "Takoradi Hospital", "TakH", "Hospital", "District", sekondiTakoradiId, "Takoradi", 4.8833, -1.7500),
                CreateFacility("44444444-4444-4444-4444-444444440191", "European Hospital Takoradi", "EHT", "Hospital", "District", sekondiTakoradiId, "Takoradi", 4.8900, -1.7600),
                CreateFacility("44444444-4444-4444-4444-444444440192", "Essikadu Health Centre", "EssHC", "Health Centre", "Sub-district", sekondiTakoradiId, "Essikadu", 4.9500, -1.7800),
            });

            // Tarkwa Nsuaem Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440193", "Tarkwa Municipal Hospital", "TarMH", "Hospital", "District", tarkwaId, "Tarkwa", 5.3000, -1.9833),
                CreateFacility("44444444-4444-4444-4444-444444440194", "Goldfields Hospital Tarkwa", "GHT", "Hospital", "Private", tarkwaId, "Tarkwa", 5.3050, -1.9800),
                CreateFacility("44444444-4444-4444-4444-444444440195", "Nsuaem Health Centre", "NsuHC", "Health Centre", "Sub-district", tarkwaId, "Nsuaem", 5.3100, -1.9900),
            });

            // Nzema East Municipal (Axim) Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440196", "Axim Government Hospital", "AGH", "Hospital", "District", axim, "Axim", 4.8667, -2.2333),
                CreateFacility("44444444-4444-4444-4444-444444440197", "Axim Health Centre", "AxHC", "Health Centre", "Sub-district", axim, "Axim", 4.8700, -2.2400),
                CreateFacility("44444444-4444-4444-4444-444444440198", "Nkroful CHPS", "NkrCHPS", "CHPS", "Community", axim, "Nkroful", 5.0833, -2.2167),
            });

            // Western North Region Facilities
            var westernNorthRegionId = Guid.Parse("11111111-1111-1111-1111-111111111116");
            var wiawsoId = Guid.Parse("22222222-2222-2222-2222-222222220242");
            var bibianiId = Guid.Parse("22222222-2222-2222-2222-222222220244");
            var juabosoId = Guid.Parse("22222222-2222-2222-2222-222222220245");

            // Sefwi Wiawso Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440199", "Sefwi Wiawso Municipal Hospital", "SWMH", "Hospital", "District", wiawsoId, "Sefwi Wiawso", 6.2167, -2.4833),
                CreateFacility("44444444-4444-4444-4444-444444440200", "St. John of God Hospital Sefwi Asafo", "SJGA", "Hospital", "District", wiawsoId, "Sefwi Asafo", 6.2300, -2.4900),
                CreateFacility("44444444-4444-4444-4444-444444440201", "Wiawso Health Centre", "WiaHC", "Health Centre", "Sub-district", wiawsoId, "Wiawso", 6.2200, -2.4850),
            });

            // Bibiani Anhwiaso Bekwai Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440202", "Bibiani Government Hospital", "BibGH", "Hospital", "District", bibianiId, "Bibiani", 6.4667, -2.3167),
                CreateFacility("44444444-4444-4444-4444-444444440203", "Sefwi Bekwai Health Centre", "SBHC", "Health Centre", "Sub-district", bibianiId, "Sefwi Bekwai", 6.4800, -2.3300),
                CreateFacility("44444444-4444-4444-4444-444444440204", "Anhwiaso CHPS", "AnhCHPS", "CHPS", "Community", bibianiId, "Anhwiaso", 6.4500, -2.3000),
            });

            // Juaboso District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440205", "Juaboso Government Hospital", "JuaGH", "Hospital", "District", juabosoId, "Juaboso", 6.4167, -2.8333),
                CreateFacility("44444444-4444-4444-4444-444444440206", "Bonsu Nkwanta Health Centre", "BNHC", "Health Centre", "Sub-district", juabosoId, "Bonsu Nkwanta", 6.4300, -2.8500),
                CreateFacility("44444444-4444-4444-4444-444444440207", "Essam CHPS", "EssCHPS", "CHPS", "Community", juabosoId, "Essam", 6.4000, -2.8200),
            });

            // Oti Region Facilities
            var otiRegionId = Guid.Parse("11111111-1111-1111-1111-111111111107");
            var krachiEastId = Guid.Parse("22222222-2222-2222-2222-222222220151");
            var nkwantaSouthId = Guid.Parse("22222222-2222-2222-2222-222222220154");

            // Krachi East Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440208", "Dambai District Hospital", "DamDH", "Hospital", "District", krachiEastId, "Dambai", 7.9833, 0.1667),
                CreateFacility("44444444-4444-4444-4444-444444440209", "Dambai Health Centre", "DamHC", "Health Centre", "Sub-district", krachiEastId, "Dambai", 7.9900, 0.1700),
                CreateFacility("44444444-4444-4444-4444-444444440210", "Tokuroano CHPS", "TokCHPS", "CHPS", "Community", krachiEastId, "Tokuroano", 7.9700, 0.1600),
            });

            // Nkwanta South Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440211", "Nkwanta District Hospital", "NkwDH", "Hospital", "District", nkwantaSouthId, "Nkwanta", 8.2500, 0.5000),
                CreateFacility("44444444-4444-4444-4444-444444440212", "Nkwanta Health Centre", "NkwHC", "Health Centre", "Sub-district", nkwantaSouthId, "Nkwanta", 8.2550, 0.5050),
                CreateFacility("44444444-4444-4444-4444-444444440213", "Damanko CHPS", "DamaCHPS", "CHPS", "Community", nkwantaSouthId, "Damanko", 8.2400, 0.4900),
            });

            // Savannah Region Facilities
            var savannahRegionId = Guid.Parse("11111111-1111-1111-1111-111111111109");
            var westGonjaId = Guid.Parse("22222222-2222-2222-2222-222222220174");
            var eastGonjaId = Guid.Parse("22222222-2222-2222-2222-222222220176");
            var boleId = Guid.Parse("22222222-2222-2222-2222-222222220179");

            // West Gonja Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440214", "Damongo District Hospital", "DamoDH", "Hospital", "District", westGonjaId, "Damongo", 9.0833, -1.8167),
                CreateFacility("44444444-4444-4444-4444-444444440215", "Damongo Health Centre", "DamoHC", "Health Centre", "Sub-district", westGonjaId, "Damongo", 9.0900, -1.8200),
                CreateFacility("44444444-4444-4444-4444-444444440216", "Larabanga CHPS", "LarCHPS", "CHPS", "Community", westGonjaId, "Larabanga", 9.2167, -1.8500),
            });

            // East Gonja Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440217", "Salaga District Hospital", "SalDH", "Hospital", "District", eastGonjaId, "Salaga", 8.5500, -0.5167),
                CreateFacility("44444444-4444-4444-4444-444444440218", "Salaga Health Centre", "SalHC", "Health Centre", "Sub-district", eastGonjaId, "Salaga", 8.5550, -0.5200),
                CreateFacility("44444444-4444-4444-4444-444444440219", "Kpandai CHPS", "KpaCHPS", "CHPS", "Community", eastGonjaId, "Kpandai", 8.4700, -0.0167),
            });

            // Bole District Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440220", "Bole District Hospital", "BolDH", "Hospital", "District", boleId, "Bole", 9.0333, -2.4833),
                CreateFacility("44444444-4444-4444-4444-444444440221", "Bole Health Centre", "BolHC", "Health Centre", "Sub-district", boleId, "Bole", 9.0400, -2.4900),
                CreateFacility("44444444-4444-4444-4444-444444440222", "Bamboi CHPS", "BamCHPS2", "CHPS", "Community", boleId, "Bamboi", 8.1500, -2.0333),
            });

            // North East Region Facilities
            var northEastRegionId = Guid.Parse("11111111-1111-1111-1111-111111111110");
            var westMamprusId = Guid.Parse("22222222-2222-2222-2222-222222220181");
            var eastMamprusId = Guid.Parse("22222222-2222-2222-2222-222222220182");

            // West Mamprusi Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440223", "Walewale District Hospital", "WalDH", "Hospital", "District", westMamprusId, "Walewale", 10.3500, -0.8000),
                CreateFacility("44444444-4444-4444-4444-444444440224", "Walewale Health Centre", "WalHC", "Health Centre", "Sub-district", westMamprusId, "Walewale", 10.3550, -0.8050),
                CreateFacility("44444444-4444-4444-4444-444444440225", "Wungu CHPS", "WunCHPS", "CHPS", "Community", westMamprusId, "Wungu", 10.3400, -0.7900),
            });

            // East Mamprusi Municipal Facilities
            facilities.AddRange(new[]
            {
                CreateFacility("44444444-4444-4444-4444-444444440226", "Nalerigu Baptist Medical Centre", "NBMC", "Hospital", "District", eastMamprusId, "Nalerigu", 10.5167, -0.3667),
                CreateFacility("44444444-4444-4444-4444-444444440227", "Gambaga Health Centre", "GamHC", "Health Centre", "Sub-district", eastMamprusId, "Gambaga", 10.5300, -0.4500),
                CreateFacility("44444444-4444-4444-4444-444444440228", "Langbinsi CHPS", "LanCHPS", "CHPS", "Community", eastMamprusId, "Langbinsi", 10.4800, -0.2000),
            });

            return facilities;
        }

        private Facility CreateFacility(string id, string name, string code, string type, string level, Guid districtId, string city, double latitude, double longitude)
        {
            var now = DateTime.UtcNow;
            return new Facility
            {
                ID = Guid.Parse(id),
                Name = name,
                Code = code,
                Type = type,
                Level = level,
                Address = $"{city}, Ghana",
                City = city,
                Country = "Ghana",
                Phone = "+233-000-000-000",
                Email = $"{code.ToLower().Replace(" ", "")}@ghs.gov.gh",
                DistrictID = districtId,
                Latitude = latitude,
                Longitude = longitude,
                GHPostGPS = "",
                IsActive = true,
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
            var korleBuId = Guid.Parse("44444444-4444-4444-4444-444444440001");
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
