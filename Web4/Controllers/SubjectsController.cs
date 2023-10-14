using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web1.CustomActionFilters;
using Web4.Data;
using Web4.Models.Domain;
using Web4.Models.Dto;


namespace Web4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly DevDbContext dbContext;

        public SubjectsController(DevDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        [Authorize(Roles = ("Writer"))]
        public async Task<IActionResult> getAllSubjectPagination([FromQuery] int current, [FromQuery] int pageSize, [FromQuery] string? subjectCode, [FromQuery] string? sort,
        [FromQuery] bool isAscending)
        {
            var subjects =  dbContext.Subjects.AsQueryable();
            if (!string.IsNullOrEmpty(subjectCode))
            {
                subjects = subjects.Where(subject => subject.SubjectCode.Contains(subjectCode));
            }
            if (string.IsNullOrWhiteSpace(sort) == false)
            {
                if (sort.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    subjects = isAscending ? subjects.OrderBy(x => x.Name) : subjects.OrderByDescending(x => x.Name);
                }
                else if (sort.Equals("Credits", StringComparison.OrdinalIgnoreCase))
                {
                    subjects = isAscending ? subjects.OrderBy(x => x.Credits) : subjects.OrderByDescending(x => x.Credits);
                }
            }
            var skipResults = (current - 1) * pageSize;
                var subjectDomain = await subjects.Skip(skipResults).Take(pageSize).ToListAsync();
                if (subjectDomain.Any())
                {
                    var result = new SubjectPaginacs
                    {
                        statuscode = 200,
                        pageSize = pageSize,
                        current = current,
                        total = subjects.LongCount(),
                        data = subjectDomain
                    };
                    return Ok(result);
            }
            return Ok(null);
        }
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> createSubject([FromBody] AddSubject addSubject)
        {
            var subject = new Subject
            {
                SubjectCode = addSubject.SubjectCode,
                Name = addSubject.Name,
                Credits = addSubject.Credits,
            };
            await dbContext.Subjects.AddAsync(subject);
            await dbContext.SaveChangesAsync();
            return Ok(subject);
        }
        [HttpDelete]
        [Route("id:Guid")]
        [ValidateModel]
        public async Task<IActionResult> deleteSubject([FromRoute] Guid id)
        {
            var result = await dbContext.Subjects.FirstOrDefaultAsync(x=>x.Id == id);
            if (result == null)
            {
                return Ok(null);
            }
            dbContext.Subjects.Remove(result);
            await dbContext.SaveChangesAsync() ;
            return Ok(result);
        }
    }
}
