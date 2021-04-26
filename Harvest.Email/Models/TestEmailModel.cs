using System.Collections.Generic;

namespace Harvest.Email.Models
{
    public class TestEmailModel
    {
        public string ButtonUrl = "https://harvest.caes.ucdavis.edu";
        public string Name { get; set; } = "@Model.Name";
        public List<string> MyList { get; set; }

        public void InitForMjml()
        {
            MyList = new List<string>();
            MyList.Add("@foreach (var item in Model.MyList){<div> @item </div>}");

        }
    }

}
