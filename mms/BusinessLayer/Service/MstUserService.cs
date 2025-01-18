
using DataAccessLayer;
using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class MstUserService : IMstUserService
    {
        private readonly IMstUserRepository _userRepository;

        public MstUserService(IMstUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<MstUser>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<MstUser> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task AddUserAsync(MstUser user)
        {
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(MstUser user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _userRepository.DeleteAsync(userId);
        }
        public async Task<bool> UserNameExistsAsync(string userName)
        {
            return await _userRepository.UserNameExistsAsync(userName);
        }

        public async Task<bool> EmailAddressExistsAsync(string emailAddress)
        {
            return await _userRepository.EmailAddressExistsAsync(emailAddress);
        }

        public async Task<bool> EmployeeIdExistsAsync(int empId)
        {
            return await _userRepository.EmployeeIdExistsAsync(empId);
        }
    }
}