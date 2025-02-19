using System.Collections.Generic;

namespace MMS.Models.Inventory
{
    public class DepartmentVM
    {
        public int ID { get; set; }
        public long DeptCode { get; set; }
        public string Name { get; set; }
        public int DeptLevel { get; set; }
        public int DeptType { get; set; }
        public string MainCostCenter { get; set; }
        public string Furniture { get; set; }
        public string OfficeEquip { get; set; }
        public string MotorVehicle { get; set; }   
        public string DeptCc { get; set; }
        public string ShorCode { get; set; }
        public long ParentCode { get; set; }
        public long BranchCode { get; set; }
        public string DeptCodeHr { get; set; }
        public bool IsActive { get; set; }
        public string DeptLevelName { get; set; }
        public string DeptTypeName { get; set; }
        public IEnumerable<int> DeptCodeIds { get; set; }
    }
}