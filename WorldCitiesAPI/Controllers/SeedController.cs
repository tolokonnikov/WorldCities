using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security;
using WorldCitiesAPI.Data.Models;

namespace WorldCitiesAPI.Controllers {

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeedController(ApplicationDbContext context, IWebHostEnvironment environment) {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Import() {

            if (!_environment.IsDevelopment())
                throw new SecurityException("Not allowed");

            var path = Path.Combine(_environment.ContentRootPath, "Data/Source/worldcities.xlsx");

            using var stream = System.IO.File.OpenRead(path);
            using var excelPackage = new ExcelPackage(stream);

            var worksheet = excelPackage.Workbook.Worksheets[0];

            var nEndRow = worksheet.Dimension.End.Row;

            var numberOfCountriesAdded = 0;
            var numberOfCitiesAdded = 0;

            var countriesByName = _context.Countries
                .AsNoTracking()
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            for (var nRow = 2; nRow < nEndRow; nRow++) {

                var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];

                var countryName = row[nRow, 5].GetValue<string>();
                var iso2 = row[nRow, 6].GetValue<string>();
                var iso3 = row[nRow, 7].GetValue<string>();

                // skip this country if it already exists in the database
                if (countriesByName.ContainsKey(countryName))
                    continue;

                // create the Country entity and fill it with xlsx data 
                var country = new Country {
                    Name = countryName,
                    ISO2 = iso2,
                    ISO3 = iso3
                };

                await _context.Countries.AddAsync(country);

                countriesByName.Add(countryName, country);

                numberOfCountriesAdded++;
            }

            if (numberOfCountriesAdded > 0)
                await _context.SaveChangesAsync();

            // create a lookup dictionary
            // containing all the cities already existing 
            // into the Database (it will be empty on first run). 
            var cities = _context.Cities
            .AsNoTracking()
            .ToDictionary(x => (
                    x.Name,
                    x.Lat,
                    x.Lon,
                    x.CountryId));

            // iterates through all rows, skipping the first one 
            for (int nRow = 2; nRow <= nEndRow; nRow++) {
                var row = worksheet.Cells[
                nRow, 1, nRow, worksheet.Dimension.End.Column];
                var name = row[nRow, 1].GetValue<string>();
                var nameAscii = row[nRow, 2].GetValue<string>();
                var lat = row[nRow, 3].GetValue<decimal>();
                var lon = row[nRow, 4].GetValue<decimal>();
                var countryName = row[nRow, 5].GetValue<string>();
                // retrieve country Id by countryName
                var countryId = countriesByName[countryName].Id;

                // skip this city if it already exists in the database
                if (cities.ContainsKey((
                        Name: name,
                        Lat: lat,
                        Lon: lon,
                        CountryId: countryId)))
                    continue;

                // create the City entity and fill it with xlsx data 
                var city = new City {
                    Name = name,
                    Lat = lat,
                    Lon = lon,
                    CountryId = countryId
                };
                // add the new city to the DB context 
                _context.Cities.Add(city);
                // increment the counter 
                numberOfCitiesAdded++;
            }
            // save all the cities into the Database 
            if (numberOfCitiesAdded > 0)
                await _context.SaveChangesAsync();

            return new JsonResult(new {
                Cities = numberOfCitiesAdded,
                Countires = numberOfCountriesAdded
            });

        }
    }
}
