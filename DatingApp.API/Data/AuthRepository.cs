using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _dataContext;
        public AuthRepository(DataContext dataContext)
        {
            this._dataContext = dataContext;

        }
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            /* Add User */
            await _dataContext.Users.AddAsync(user);
            /* Save */
            await _dataContext.SaveChangesAsync();

            return user;
        }

        /**
        Create hash and get salt.
        Assigned to variables, passed in by reference.
        */
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            /* HMACSHA512 implements an interface IDisposable which has a method Dispose().  
               This performs application-defined tasks associated with freeing, releasing, or resetting
               unmanaged resources. So we 'using' */
            using (var hmac = new System.Security.Cryptography.HMACSHA512()){
                //hmac gives us a randomly generated key which we use as the salt and save it.
                passwordSalt = hmac.Key;
                // convert password to bytes
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _dataContext.Users.AnyAsync(user => user.Username == username))
                return true;

            return false;

        }
        public async Task<User> Login(string username, string password)
        {
             var user = await _dataContext.Users.FirstOrDefaultAsync(user => user.Username == username);

            if(user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
                return null;
            }

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            //Create new instance of HMACSH512 but use the salt key in the constructor.
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i=0; i <computedHash.Length; i ++) {
                    if (computedHash[i] == passwordHash[i])
                        return false;
                }
            }

            return true;
        }
    }
}