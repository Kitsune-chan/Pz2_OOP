using System;
using System.Collections.Generic;
using System.Linq;

interface IPerson
{
    string Name { get; }
    string Patronomic { get; }
    string Lastname { get; }
    DateTime Date { get; }
    int Age { get; }
}

public enum Position
{
    Assistant,
    SeniorLecturer,
    Docent,
    Professor,
    DepartmentHead
}

class Student : IPerson
{
    public string Name { get; }
    public string Patronomic { get; }
    public string Lastname { get; }
    public DateTime Date { get; }
    public int Age => DateTime.Now.Year - Date.Year;
    public int Course { get; }
    public string Group { get; }
    public float AverageScore { get; }

    public Student(string name, string patronomic, string lastname, DateTime date, int course, string group, float averageScore)
    {
        Name = name;
        Patronomic = patronomic;
        Lastname = lastname;
        Date = date;
        Course = course;
        Group = group;
        AverageScore = averageScore;
    }

    public static Student CreateFromString(string data)
    {
        var parts = data.Split(',');
        if (parts.Length != 7)
            throw new ArgumentException("Неверный формат данных для студента");

        return new Student(
            parts[0],
            parts[1],
            parts[2],
            DateTime.Parse(parts[3]),
            int.Parse(parts[4]),
            parts[5],
            float.Parse(parts[6])
        );
    }

    public override string ToString()
    {
        return $"Студент: {Lastname} {Name} {Patronomic}, Возраст: {Age}, Курс: {Course}, Группа: {Group}, Средний балл: {AverageScore:F2}";
    }
}

class Teacher : IPerson
{
    public string Name { get; }
    public string Patronomic { get; }
    public string Lastname { get; }
    public DateTime Date { get; }
    public int Age => DateTime.Now.Year - Date.Year;
    public string Department { get; }
    public int Experience { get; }
    public Position Position { get; }

    public Teacher(string name, string patronomic, string lastname, DateTime date, string department, int experience, Position position)
    {
        Name = name;
        Patronomic = patronomic;
        Lastname = lastname;
        Date = date;
        Department = department;
        Experience = experience;
        Position = position;
    }

    public static Teacher CreateFromString(string data)
    {
        var parts = data.Split(',');
        if (parts.Length != 7)
            throw new ArgumentException("Неверный формат данных для преподавателя");

        return new Teacher(
            parts[0],
            parts[1],
            parts[2],
            DateTime.Parse(parts[3]),
            parts[4],
            int.Parse(parts[5]),
            (Position)Enum.Parse(typeof(Position), parts[6])
        );
    }

    public override string ToString()
    {
        return $"Преподаватель: {Lastname} {Name} {Patronomic}, Возраст: {Age}, Кафедра: {Department}, Стаж: {Experience}, Должность: {Position}";
    }
}

interface IUniversity
{
    IEnumerable<IPerson> Persons { get; }
    IEnumerable<Student> Students { get; }
    IEnumerable<Teacher> Teachers { get; }

    void Add(IPerson person);
    void Remove(IPerson person);
    IEnumerable<IPerson> FindByLastName(string lastName);
    IEnumerable<Student> FindByAvrPoint(float avrPoint);
    IEnumerable<Teacher> FindByDepartment(string text);
}

class University : IUniversity
{
    private List<IPerson> persons = new List<IPerson>();

    public IEnumerable<IPerson> Persons => persons.OrderBy(p => p.Lastname).ThenBy(p => p.Name);
    public IEnumerable<Student> Students => persons.OfType<Student>().OrderBy(s => s.Course).ThenBy(s => s.Group);
    public IEnumerable<Teacher> Teachers => persons.OfType<Teacher>().OrderBy(t => t.Department).ThenBy(t => t.Lastname);

    public void Add(IPerson person)
    {
        persons.Add(person);
    }

    public void Remove(IPerson person)
    {
        persons.Remove(person);
    }

    public IEnumerable<IPerson> FindByLastName(string lastName)
    {
        return persons.Where(p => p.Lastname.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(p => p.Lastname).ThenBy(p => p.Name);
    }

    public IEnumerable<Student> FindByAvrPoint(float avrPoint)
    {
        return persons.OfType<Student>()
                     .Where(s => s.AverageScore > avrPoint)
                     .OrderByDescending(s => s.AverageScore);
    }

    public IEnumerable<Teacher> FindByDepartment(string text)
    {
        return persons.OfType<Teacher>()
                     .Where(t => t.Department.Contains(text, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(t => t.Position);
    }
}

class Program
{
    static University university = new University();

    static void Main(string[] args)
    {
        InitializeTestData();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Университетская система ===");
            Console.WriteLine("1. Показать всех людей");
            Console.WriteLine("2. Показать всех студентов");
            Console.WriteLine("3. Показать всех преподавателей");
            Console.WriteLine("4. Добавить студента");
            Console.WriteLine("5. Добавить преподавателя");
            Console.WriteLine("6. Найти по фамилии");
            Console.WriteLine("7. Найти студентов с баллом выше заданного");
            Console.WriteLine("8. Найти преподавателей по кафедре");
            Console.WriteLine("9. Удалить человека");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите пункт меню: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    ShowAllPersons();
                    break;
                case "2":
                    ShowAllStudents();
                    break;
                case "3":
                    ShowAllTeachers();
                    break;
                case "4":
                    AddStudent();
                    break;
                case "5":
                    AddTeacher();
                    break;
                case "6":
                    FindByLastName();
                    break;
                case "7":
                    FindByAvrPoint();
                    break;
                case "8":
                    FindByDepartment();
                    break;
                case "9":
                    RemovePerson();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    WaitForKey();
                    break;
            }
        }
    }

    static void InitializeTestData()
    {
        university.Add(new Student("Иван", "Петрович", "Сидоров", new DateTime(2000, 5, 15), 2, "ИСП-201", 4.2f));
        university.Add(new Student("Мария", "Сергеевна", "Иванова", new DateTime(2001, 8, 22), 1, "ИСП-101", 4.7f));
        university.Add(new Student("Алексей", "Владимирович", "Петров", new DateTime(1999, 3, 10), 3, "ИСП-301", 3.8f));

        university.Add(new Teacher("Ольга", "Николаевна", "Смирнова", new DateTime(1975, 12, 5), "Информационные системы", 15, Position.Professor));
        university.Add(new Teacher("Дмитрий", "Иванович", "Козлов", new DateTime(1980, 7, 18), "Программная инженерия", 10, Position.Docent));
        university.Add(new Teacher("Елена", "Викторовна", "Павлова", new DateTime(1985, 2, 28), "Информационные системы", 8, Position.SeniorLecturer));
    }

    static void ShowAllPersons()
    {
        Console.WriteLine("\n=== Все люди в университете ===");
        foreach (var person in university.Persons)
        {
            Console.WriteLine(person);
        }
        WaitForKey();
    }

    static void ShowAllStudents()
    {
        Console.WriteLine("\n=== Все студенты ===");
        foreach (var student in university.Students)
        {
            Console.WriteLine(student);
        }
        WaitForKey();
    }

    static void ShowAllTeachers()
    {
        Console.WriteLine("\n=== Все преподаватели ===");
        foreach (var teacher in university.Teachers)
        {
            Console.WriteLine(teacher);
        }
        WaitForKey();
    }

    static void AddStudent()
    {
        try
        {
            Console.WriteLine("\n=== Добавление студента ===");
            Console.WriteLine("Введите данные в формате: Имя,Отчество,Фамилия,ДатаРождения(гггг-мм-дд),Курс,Группа,СреднийБалл");
            Console.Write("Данные: ");
            var input = Console.ReadLine();

            var student = Student.CreateFromString(input);
            university.Add(student);
            Console.WriteLine("Студент успешно добавлен!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        WaitForKey();
    }

    static void AddTeacher()
    {
        try
        {
            Console.WriteLine("\n=== Добавление преподавателя ===");
            Console.WriteLine("Введите данные в формате: Имя,Отчество,Фамилия,ДатаРождения(гггг-мм-дд),Кафедра,Стаж,Должность");
            Console.WriteLine("Должности: Assistant, SeniorLecturer, Docent, Professor, DepartmentHead");
            Console.Write("Данные: ");
            var input = Console.ReadLine();

            var teacher = Teacher.CreateFromString(input);
            university.Add(teacher);
            Console.WriteLine("Преподаватель успешно добавлен!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        WaitForKey();
    }

    static void FindByLastName()
    {
        Console.WriteLine("\n=== Поиск по фамилии ===");
        Console.Write("Введите фамилию: ");
        var lastName = Console.ReadLine();

        var results = university.FindByLastName(lastName);
        if (results.Any())
        {
            foreach (var person in results)
            {
                Console.WriteLine(person);
            }
        }
        else
        {
            Console.WriteLine("Люди с такой фамилией не найдены.");
        }
        WaitForKey();
    }

    static void FindByAvrPoint()
    {
        Console.WriteLine("\n=== Поиск студентов с баллом выше заданного ===");
        Console.Write("Введите минимальный средний балл: ");
        if (float.TryParse(Console.ReadLine(), out float avrPoint))
        {
            var results = university.FindByAvrPoint(avrPoint);
            if (results.Any())
            {
                foreach (var student in results)
                {
                    Console.WriteLine(student);
                }
            }
            else
            {
                Console.WriteLine("Студенты с таким баллом не найдены.");
            }
        }
        else
        {
            Console.WriteLine("Неверный формат балла!");
        }
        WaitForKey();
    }

    static void FindByDepartment()
    {
        Console.WriteLine("\n=== Поиск преподавателей по кафедре ===");
        Console.Write("Введите текст для поиска в названии кафедры: ");
        var text = Console.ReadLine();

        var results = university.FindByDepartment(text);
        if (results.Any())
        {
            foreach (var teacher in results)
            {
                Console.WriteLine(teacher);
            }
        }
        else
        {
            Console.WriteLine("Преподаватели по такой кафедре не найдены.");
        }
        WaitForKey();
    }

    static void RemovePerson()
    {
        Console.WriteLine("\n=== Удаление человека ===");
        Console.WriteLine("Список всех людей:");

        var personsList = university.Persons.ToList();
        for (int i = 0; i < personsList.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {personsList[i]}");
        }

        Console.Write("Введите номер для удаления: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= personsList.Count)
        {
            university.Remove(personsList[index - 1]);
            Console.WriteLine("Человек успешно удален!");
        }
        else
        {
            Console.WriteLine("Неверный номер!");
        }
        WaitForKey();
    }

    static void WaitForKey()
    {
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }
}
