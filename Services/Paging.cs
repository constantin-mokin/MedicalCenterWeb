namespace MedicalCenterWeb.Services
{
    public static class Paging
    {
        public static int Normalize(int page) => page < 1 ? 1 : page;

        public static int TotalPages(int totalItems, int pageSize) =>
            (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}
