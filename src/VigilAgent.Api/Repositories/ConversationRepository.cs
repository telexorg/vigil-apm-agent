using BloggerAgent.Domain.Commons;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Data;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;

namespace VigilAgent.Api.Repositories
{
    public class ConversationRepository : TelexRepository<Message>, IConversationRepository
    {
        private readonly TelexDbContext _context;
        private readonly ITelexRepository<Message> _repository;

        public ConversationRepository(TelexDbContext context, ITelexRepository<Message> repository) : base(context)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<Document<Message>> GetConversationsByUserAsync(string userId)
        {
            return await _repository.GetByIdAsync(userId);
        }

        public async Task<List<TelexChatMessage>> GetMessagesAsync(string contextId)
        {
            var conversations = await _repository.FilterAsync(new { tag = CollectionType.Message });
            if (conversations == null)
            {
                throw new Exception("Failed to retrieve messages");
            }

            return conversations
                .Where(c => c.Data.ContextId == contextId)
                .Select(c => new TelexChatMessage()
                {
                    Role = c.Data.Role,
                    Content =  c.Data.Content 
                }).ToList();
        }
             

        //public async Task<Company> AddCompanyAsync(Company company)
        //{
        //    if (company.Id == null)
        //        throw new ArgumentNullException();

        //    var existingCompany = await _companyRepository.GetByIdAsync(company.Id);

        //    if (existingCompany != null)
        //    {
        //        throw new Exception("Company already exists");
        //    }

        //    var newCompany = new Company
        //    {
        //        Id = company.Id,
        //        Name = company.Name,
        //        Tone = company.Tone,
        //        TargetAudience = company.TargetAudience,
        //        Overview = company.Overview,
        //        Industry = company.Industry,
        //    };

        //    /*var channelId = company.Users.FirstOrDefault().Id;

        //    var user = await _userService.GetUserAsync(channelId);

        //    if (user != null)
        //    {
        //        throw new DuplicateNameException();
        //    }
        //    var newUser = new User { Id = channelId };

        //    newCompany.Users.Add(newUser);

        //    try
        //    {
        //    }
        //    catch (Exception)
        //    {
        //        // Rollback: Delete the company if user creation fails
        //        await _userRepository.DeleteAsync(newUser.Id);
        //        throw;
        //    }*/

        //    // Register the company’s communication channel as a user
        //    await _companyRepository.CreateAsync(newCompany);

        //    return newCompany;
        //}

        //public async Task<Company> UpdateCompanyAsync(Company company)
        //{
        //    if (company.Id == null)
        //        throw new ArgumentNullException();

        //    var companyDoc = await _companyRepository.GetByIdAsync(company.Id);

        //    if (companyDoc == null)
        //    {
        //        throw new Exception("Company not found exists");
        //    }

        //    var existingCompany = companyDoc.Data;

        //    existingCompany.Id = company.Id ?? existingCompany.Id;
        //    existingCompany.Name = company.Name ?? existingCompany.Name;
        //    existingCompany.Tone = company.Tone ?? existingCompany.Tone;
        //    existingCompany.TargetAudience = company.TargetAudience ?? existingCompany.TargetAudience;
        //    existingCompany.Overview = company.Overview ?? existingCompany.Overview;
        //    existingCompany.Industry = company.Industry ?? existingCompany.Industry;



        //    // Register the company’s communication channel as a user
        //    await _companyRepository.UpdateAsync("", existingCompany);

        //    return existingCompany;
        //}
    }
}

