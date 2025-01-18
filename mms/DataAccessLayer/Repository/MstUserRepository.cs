using SharedLayer.AB_Common;
using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class MstUserRepository : IMstUserRepository
    {
        private readonly MasterDataContext _context;

        public MstUserRepository(MasterDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MstUser>> GetAllAsync()
        {
            return await _context.MstUsers.ToListAsync();
        }

        public async Task<MstUser> GetByIdAsync(int userId)
        {
            return await _context.MstUsers.FindAsync(userId);
        }            

        public MstUser UpdateLogin(MstUser usermaster)
        {
            try
            {
                long result = -1;
                var results = _context.MstUsers.SingleOrDefault(x => x.UserId == usermaster.UserId);
                if (usermaster != null)
                {
                    usermaster.LastLogin = DateTime.Now;
                    usermaster.SessionID = Guid.NewGuid();
                    _context.SaveChanges();
                    result = usermaster.UserId;
                }
                return usermaster;
            }
            catch (Exception e)
            {
                Exception inner = e.InnerException ?? e;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                Log.Error(inner.Message);
                throw;
            }
        }
        public async Task AddAsync(MstUser user)
        {
           // await _context.MstUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstUser user)
        {
            _context.Entry(user).State = (System.Data.Entity.EntityState)System.Data.Entity.EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var user = await _context.MstUsers.FindAsync(userId);
            if (user != null)
            {
                _context.MstUsers.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserNameExistsAsync(string userName)
        {
            
            var user=await _context.MstUsers.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
       
        public async Task<MstUser> GetByUsernameAsync(string username)
        {
            return await _context.MstUsers.SingleOrDefaultAsync(u => u.UserName == username);
        }
        public MstUser GetByUsername(string username)
        {
            return _context.MstUsers.SingleOrDefault(u => u.UserName == username);
        }
        public async Task<bool> EmailAddressExistsAsync(string emailAddress)
        {
            return await _context.MstUsers.AnyAsync(u => u.EmailAddress == emailAddress);
        }

        public async Task<bool> EmployeeIdExistsAsync(int empId)
        {
            return await _context.MstUsers.AnyAsync(u => u.EmpId == empId);
        }
    }
}
