using BusinessLayer.Interfaces;
using MMS.Models;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;
using SharedLayer.Models;
using SharedLayer.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class JournalController : BaseController
    {
        private readonly IJournalService _journalService;
        private readonly ICommonService _commonService;
        private readonly SystemTools _systemTools = new SystemTools();

        public JournalController(ILogHistoryService logService, ICommonService commonService, IJournalService journalService) : base(logService)
        {
            _journalService = journalService;
            _commonService = commonService;
        }

        // GET: Journal
        public async Task<ActionResult> GetJournal()
        {
            try
            {
                string userId = Session["UserId"]?.ToString();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["Error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Login");
                }

                var journalList = await _journalService.GetJournalEntriesAsync(userId);
                var journalVMList = journalList.Select(entry => new JournalVM
                {
                    RefNo = entry.RefNo,
                    Description = entry.Description,
                    StoreName = entry.StoreName,
                    TotalAmount = entry.TotalAmount,
                    UserName = Session["UserName"]?.ToString()
                }).ToList();

                return View(journalVMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.ToString());
                return RedirectToAction("GetJournal");
            }
        }
        public async Task<ActionResult> GetJournalYdt()
        {
            try
            {
                string userId = Session["UserId"]?.ToString();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["Error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Login");
                }

                var journalListYdt = await _journalService.GetJournalYdtEntriesAsync(userId);
                var journalVMList = journalListYdt.Select(entry => new JournalVM
                {
                    RefNo = entry.RefNo,
                    Description = entry.Description,
                    StoreName = entry.StoreName,
                    TotalAmount = entry.TotalAmount,
                    UserName = Session["UserName"]?.ToString()
                }).ToList();

                return View(journalVMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.ToString());
                return RedirectToAction("GetJournalYdt");
            }
        }       
        // GET: Journal/Details/{refNo}
        public async Task<ActionResult> GetJournalDetails(string refNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                    return new HttpStatusCodeResult(400, "Invalid reference number.");

                var journalDetails = await _journalService.GetItemEntriesAsync(refNo);
                if (journalDetails == null || !journalDetails.Any())
                {
                    TempData["Error"] = "No journal details found for the provided reference number.";
                    Log.Warn($"No journal entries found for RefNo: {refNo}");
                    return RedirectToAction("GetUnJournal");
                }
                var journalDetailsVM = journalDetails.Select(detail => new JournalVM
                {
                    DetailId = detail.DetailId,
                    RefNo = detail.RefNo,
                    Description = detail.Description,
                    Department = detail.Department,
                    Quantity = detail.Quantity,
                    UnitCost = detail.UnitCost,
                    TotalAmount = detail.TotalAmount,
                    CostAccountNo = detail.CostAccountNo,
                    StockAccountNo = detail.StockAccountNo,
                }).ToList();

                return View("JournalDetails", journalDetailsVM);
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.ToString());
                return new HttpStatusCodeResult(500, "An error occurred while fetching journal details.");
            }
        }
        public async Task<ActionResult> GetJournalDetailsYdt(string refNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                    return new HttpStatusCodeResult(400, "Invalid reference number.");

                var journalDetails = await _journalService.GetItemYdtEntriesAsync(refNo);
                if (journalDetails == null || !journalDetails.Any())
                {
                    TempData["Error"] = "No journal details found for the provided reference number.";
                    Log.Warn($"No journal entries found for RefNo: {refNo}");
                    return RedirectToAction("GetUnJournal");
                }

                var journalDetailsVM = journalDetails.Select(detail => new JournalVM
                {
                    DetailId = detail.DetailId,
                    RefNo = detail.RefNo,
                    Description = detail.Description,
                    Department = detail.Department,
                    Quantity = detail.Quantity,
                    UnitCost = detail.UnitCost,
                    TotalAmount = detail.TotalAmount,
                    CostAccountNo = detail.CostAccountNo,
                    StockAccountNo = detail.StockAccountNo,
                }).ToList();
                return View("JournalDetailsYdt", journalDetailsVM);
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.ToString());
                return new HttpStatusCodeResult(500, "An error occurred while fetching journal details.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveJournalTransaction(string refNo)
        {
            try
            {
                string Ydt = "notclosed";
                if (string.IsNullOrWhiteSpace(refNo))
                {
                    TempData["Error"] = "Reference number is required.";
                    return RedirectToAction("GetJournal");
                }

                var journalDetails = await _journalService.GetItemEntriesAsync(refNo);

                if (journalDetails == null || !journalDetails.Any())
                {
                    TempData["Error"] = "No journal details found for the provided reference number.";
                    return RedirectToAction("GetJournal");
                }

                var totalAmount = journalDetails.Sum(detail => detail.TotalAmount);
                var department = journalDetails.First().Department;

                var journalEntry = new TblJournal
                {
                    RefNo = refNo,
                    Department = department,
                    Quantity = journalDetails.Sum(detail => detail.Quantity),
                    TotalAmount = totalAmount,
                    CostAccountNo = journalDetails.First().CostAccountNo,
                    StockAccountNo = journalDetails.First().StockAccountNo,
                    CrtBy = Session["UserName"]?.ToString(),
                    CrtDt = DateTime.Now,
                    DateEth = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Now),
                    PeriodId = _systemTools.GetCurrentPeriod()
                };

                bool saveResult = await _journalService.SaveJournalEntryAsync(journalEntry);

                var changes = new List<string>
                {
                    $"RefNo: {refNo}",
                    $"Department: {department}",
                    $"TotalAmount: {totalAmount}",
                    $"Quantity: {journalEntry.Quantity}",
                    $"CostAccountNo: {journalEntry.CostAccountNo}",
                    $"StockAccountNo: {journalEntry.StockAccountNo}"
                };

                if (changes.Any())
                {
                    string logSuccess = $"Journal transaction for RefNo '{refNo}' saved by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                        "Changes made:\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(refNo, "Journal", logSuccess, Session["UserName"].ToString());
                }
                return RedirectToAction("PrintPreview", new { refNo, Ydt });
            }
            catch (Exception ex)
            {
                Log.Error("Error while saving journal transaction. " + ex.Message);
                TempData["Error"] = ex.Message;
                return RedirectToAction("GetJournal");
            }
        }
        [HttpPost]
        public async Task<ActionResult> SaveJournalYdtTransaction(string refNo)
        {
            try
            {
                string Ydt = "closedperiod";
                if (string.IsNullOrWhiteSpace(refNo))
                {
                    TempData["Error"] = "Reference number is required.";
                    return RedirectToAction("GetJournal");
                }

                var journalDetails = await _journalService.GetItemYdtEntriesAsync(refNo);

                if (journalDetails == null || !journalDetails.Any())
                {
                    TempData["Error"] = "No journal details found for the provided reference number.";
                    return RedirectToAction("GetJournalYdt");
                }

                var totalAmount = journalDetails.Sum(detail => detail.TotalAmount);
                var department = journalDetails.First().Department;

                var journalEntry = new TblJournalYtd
                {
                    RefNo = refNo,
                    Department = department,
                    TotalAmount = totalAmount,
                    CostAccountNo = journalDetails.First().CostAccountNo,
                    StockAccountNo = journalDetails.First().StockAccountNo,
                    CrtBy = Session["UserName"]?.ToString(),
                    CrtDt = DateTime.Now,
                    DateEth = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Now),
                    PeriodId = _systemTools.GetCurrentPeriod()
                };

                bool saveResult = await _journalService.SaveJournalYdtEntryAsync(journalEntry);

                var changes = new List<string>
                {
                    $"RefNo: {refNo}",
                    $"Department: {department}",
                    $"TotalAmount: {totalAmount}",
                    $"CostAccountNo: {journalEntry.CostAccountNo}",
                    $"StockAccountNo: {journalEntry.StockAccountNo}"
                };

                if (changes.Any())
                {
                    string logSuccess = $"Journal transaction for RefNo '{refNo}' saved by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                        "Changes made:\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(refNo, "Journal", logSuccess, Session["UserName"].ToString());
                }

                return RedirectToAction("PrintPreview", new { refNo ,Ydt});
            }
            catch (Exception ex)
            {
                Log.Error("Error while saving journal transaction. " + ex.Message);
                TempData["Error"] = ex.Message;
                return RedirectToAction("GetJournalYdt");
            }
        }

        public async Task<ActionResult> GetUnJournal()
        {
            try
            {
                string userId = Session["UserId"]?.ToString();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["Error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Login");
                }

                var unJournalEntries = await _journalService.GetUnJournalEntriesAsync(userId);

                if (unJournalEntries == null || !unJournalEntries.Any())
                {
                    TempData["Error"] = "No unjournal entries found.";
                    return View(new List<JournalVM>());
                }

                // Map database entities to the JournalVM
                var journalVMList = unJournalEntries.Select(entry => new JournalVM
                {
                    RefNo = entry.RefNo,
                    StoreName = entry.StoreName,
                    StockAccountNo = entry.StockAccountNo,
                    CostAccountNo = entry.CostAccountNo,
                    TotalAmount = entry.Debit,
                    UserName = Session["UserName"]?.ToString()
                }).ToList();

                return View(journalVMList);
            }
            catch (Exception ex)
            {
                Log.Error("Error while fetching unjournal entries. " + ex.Message);
                TempData["Error"] = "An error occurred while fetching unjournal entries.";
                return RedirectToAction("GetUnJournal");
            }
        }
        public async Task<ActionResult> GetUnJournalYdt(string userId, string searchTerm = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                userId = Session["UserId"]?.ToString();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["Error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Login");
                }

                var unJournalEntries = await _journalService.GetUnJournalYdtEntriesAsync(userId, searchTerm, pageNumber, pageSize);

                if (unJournalEntries == null || !unJournalEntries.Any())
                {
                    TempData["Error"] = "No unjournal entries found.";
                    return View(new List<JournalVM>());
                }

                var journalVMList = unJournalEntries.Select(entry => new JournalVM
                {
                    RefNo = entry.RefNo,
                    StoreName = entry.StoreName,
                    StockAccountNo = entry.StockAccountNo,
                    CostAccountNo = entry.CostAccountNo,
                    TotalAmount = entry.Debit,
                    UserName = Session["UserName"]?.ToString()
                }).ToList();

                // Update ViewBag values for pagination and search tracking
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.SearchTerm = searchTerm;

                return View(journalVMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.ToString());
                return RedirectToAction("GetUnJournalYdt");
            }
        }
        [HttpPost]
        public async Task<ActionResult> SaveUnJournalTransaction(string refNo)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                {
                    TempData["Error"] = "Reference number is required.";
                    Log.Warn("Reference number is missing.");
                    return RedirectToAction("GetUnJournal");
                }


                var journalDetails = await _journalService.GetItemEntriesAsync(refNo);

                if (journalDetails == null || !journalDetails.Any())
                {
                    TempData["Error"] = "No journal details found for the provided reference number.";
                    Log.Warn($"No journal entries found for RefNo: {refNo}");
                    return RedirectToAction("GetUnJournal");
                }

                // Prepare data for logging changes
                var changes = new List<string>
                {
                    $"RefNo: {refNo}",
                    $"Un-journalized By: {Session["UserName"]}",
                    $"Un-journalized Date: {DateTime.Now}"
                };

                // Un-journal the entry
                var unjournalEntry = new TblJournal
                {
                    RefNo = refNo,
                    CrtBy = Session["UserName"]?.ToString(),
                    CrtDt = DateTime.Now
                };

                var result = await _journalService.SaveUnJournalEntryAsync(unjournalEntry);

                if (!result)
                {
                    TempData["Error"] = "An error occurred while un-journalizing the entry.";
                    Log.Error($"Failed to un-journal RefNo: {refNo}");
                    return RedirectToAction("GetUnJournal");
                }

                // Log successful un-journalization
                string logSuccess = $"Journal entry '{refNo}' un-journalized by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                    "Changes made:\n" +
                                    string.Join("\n", changes);
                await LogActionAsync(refNo, "Un-Journal", logSuccess, Session["UserName"].ToString());

                TempData["Success"] = $"Journal entry '{refNo}' has been successfully un-journalized.";
                Log.Info($"Successfully un-journalized RefNo: {refNo}");
                return RedirectToAction("GetUnJournal");
            }
            catch (Exception ex)
            {
                Log.Error($"Error while un-journalizing the entry with RefNo: {refNo}: {ex.Message}", ex);
                TempData["Error"] = "An error occurred while processing your request.";
                return RedirectToAction("GetUnJournal");
            }
            finally
            {
                // Log the end of the action
                Log.Info($"SaveUnJournalTransaction ended for RefNo: {refNo}.");
            }
        }
        [HttpPost]
        public async Task<ActionResult> SaveUnJournalYdtTransaction(string refNo)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                {
                    TempData["Error"] = "Reference number is required.";
                    Log.Warn("Reference number is missing.");
                    return RedirectToAction("GetUnJournal");
                }


                var journalDetails = await _journalService.GetItemYdtEntriesAsync(refNo);

                if (journalDetails == null || !journalDetails.Any())
                {
                    TempData["Error"] = "No journal details found for the provided reference number.";
                    Log.Warn($"No journal entries found for RefNo: {refNo}");
                    return RedirectToAction("GetUnJournal");
                }

                // Prepare data for logging changes
                var changes = new List<string>
                {
                    $"RefNo: {refNo}",
                    $"Un-journalized By: {Session["UserName"]}",
                    $"Un-journalized Date: {DateTime.Now}"
                };

                // Un-journal the entry
                var unjournalEntry = new TblJournalYtd
                {
                    RefNo = refNo,
                    CrtBy = Session["UserName"]?.ToString(),
                    CrtDt = DateTime.Now
                };

                var result = await _journalService.SaveUnJournalYdtEntryAsync(unjournalEntry);

                if (!result)
                {
                    TempData["Error"] = "An error occurred while un-journalizing the entry.";
                    Log.Error($"Failed to un-journal RefNo: {refNo}");
                    return RedirectToAction("GetUnJournal");
                }

                // Log successful un-journalization
                string logSuccess = $"Journal entry '{refNo}' un-journalized by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                    "Changes made:\n" +
                                    string.Join("\n", changes);
                await LogActionAsync(refNo, "Un-Journal", logSuccess, Session["UserName"].ToString());

                TempData["Success"] = $"Journal entry '{refNo}' has been successfully un-journalized.";
                Log.Info($"Successfully un-journalized RefNo: {refNo}");
                return RedirectToAction("GetUnJournal");
            }
            catch (Exception ex)
            {
                Log.Error($"Error while un-journalizing the entry with RefNo: {refNo}: {ex.Message}", ex);
                TempData["Error"] = "An error occurred while processing your request.";
                return RedirectToAction("GetUnJournal");
            }
            finally
            {
                // Log the end of the action
                Log.Info($"SaveUnJournalTransaction ended for RefNo: {refNo}.");
            }
        }
        public ActionResult PrintPreview(string RefNo, string Ydt)
        {
            return Redirect($"~/ReportViewers/JournalReportViewer.aspx?Id={RefNo}&Ydt={Ydt}");
        }
        private void LogException(Exception ex, string Success)
        {
            Log.Error(Success, ex);
            TempData["Error"] = Success;
        }
    }
}
