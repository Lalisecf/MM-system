using System.Threading.Tasks;
using System.Web.Mvc;
using DataAccessLayer;
using SharedLayer.Models;

namespace MMS.Controllers
{
    public class MstUserController : Controller
    {
        private readonly IMstUserRepository _userRepository;

        public MstUserController(IMstUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: MstUser
        public async Task<ActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // GET: MstUser/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // GET: MstUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MstUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MstUser user)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.AddAsync(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: MstUser/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // POST: MstUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MstUser user)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.UpdateAsync(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: MstUser/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // POST: MstUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _userRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
