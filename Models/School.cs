namespace MentorsAndStudents
{
    public class School
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public GradingSystem GradingSystem { get; set; }
    }

    public enum GradingSystem
    {
        OneToFive,       // 1–5
        OneToSix,        // 1–6
        OneToTen,        // 1–10
        ZeroToHundred,   // 0–100, процентная шкала
        AF,              // A, B, C, D, E, F
        ECTS,            // Европейская шкала: A, B, C, D, E, FX, F
        Descriptive      // Оценки словами: "Отлично", "Хорошо", и т.д.
    }

    public static class GradingSystemDescriptions
    {
        public static readonly Dictionary<GradingSystem, string> Descriptions = new()
        {
            { GradingSystem.OneToFive, "Scale from 1 to 5 (Russia, Germany, Austria)" },
            { GradingSystem.OneToSix, "Scale from 1 to 6 (Switzerland, Finland)" },
            { GradingSystem.OneToTen, "Scale from 1 to 10 (Netherlands, Italy, Portugal)" },
            { GradingSystem.ZeroToHundred, "Scale from 0 to 100% (Israel, USA, Canada, Japan)" },
            { GradingSystem.AF, "Letter grades A to F (USA, international schools)" },
            { GradingSystem.ECTS, "European ECTS grading scale A to F/FX (EU universities)" },
            { GradingSystem.Descriptive, "Descriptive grading (e.g., Excellent, Good) — used in some elementary schools" }
        };
    }
}
