namespace WS_CMVC_Demo.Models.UsersViewModels
{
    public class BadgeViewModel
    {
        public Guid? Id { get; set; }

        public string BadgeColor { get; set; }

        public string BadgeTextColor { get; set; }

        public string SecondName { get; set; }

        public string Name { get; set; }

        public string MiddleName { get; set; }

        public string RussiaSubject { get; set; }

        public string Country { get; set; }

        public string Competence { get; set; }

        public string Subcategory { get; set; }

        public IEnumerable<Pictogram> Pictograms { get; set; }
    }

    public class Pictogram
    {
        public string IcoUrl { get; set; }

        public int? Order { get; set; }
    }
}
