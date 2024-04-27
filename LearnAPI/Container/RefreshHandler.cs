using LearnAPI.Repos;
using LearnAPI.Service;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LearnAPI.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly LearndataContext context;
        public RefreshHandler(LearndataContext context) 
        { 
            this.context = context;
        }


        public async Task<string> GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using(var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string refreshtoken = Convert.ToBase64String(randomnumber);
                var Existtoken = this.context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username);
                if (Existtoken != null)
                {
                    Existtoken.RefreshToken = refreshtoken;
                }
                return refreshtoken;
            }
        }
    }
}
