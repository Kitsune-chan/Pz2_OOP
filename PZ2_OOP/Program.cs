using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

interface IPerson
{
    string Lastname { get; }
    string Name { get; }
    string Patronomic { get; }
    DateTime Date { get; }
    int Age { get; }
}

public class Person : IPerson
{
    public string Lastname { get; protected set; }
    public string Name { get; protected set; }
    public string Patronomic { get; protected set; }

    public DateTime Date { get; protected set; }

    public int Age
    {
        get
        {
            DateTime today = DateTime.Today;
            int age = today.Year - Date.Year;

            if (Date.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }

    public Person(string lastName, string firstName, string middleName, DateTime birthDate)
    {
        Lastname = lastName;
        Name = firstName;
        Patronomic = middleName;
        Date = birthDate;
    }

    public static Person Parse(string text)
    {
        var columns = text.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);

        if (columns.Length != 4)
            throw new FormatException("Неверное количество колонок");

        try
        {
            var lastname = columns[0].Trim();
            var firstname = columns[1].Trim();
            var patronomic = columns[2].Trim();
            var date = DateTime.ParseExact(columns[3].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

            return new Person(lastname, firstname, patronomic, date);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Ошибка парсинга данных: {ex.Message}", ex);
        }
    }

    public override string ToString()
    {
        return $"{Lastname}, {Name}, {Patronomic}, {Date:dd-MM-yyyy}, {Age}";
    }
}

public enum Position
{
    Assistant,
    SeniorLecturer,
    Docent,
    Professor,
    DepartmentHead
}

class Student : Person
{
    public int Course { get; }
    public string Group { get; }
    public float AverageScore { get; }


    public Student(string lastName, string firstName, string middleName, DateTime date,
                  int course, string group, float averageScore)
        : base(lastName, firstName, middleName, date)
    {
        Course = course;
        Group = group;
        AverageScore = averageScore;
    }

    public static new Student Parse(string text)
    {
        var columns = text.Split([';'], StringSplitOptions.RemoveEmptyEntries);

        if (columns.Length != 7)
            throw new FormatException("Неверное количество колонок для студента");

        try
        {
            var lastname = columns[0].Trim();
            var firstname = columns[1].Trim();
            var patronomic = columns[2].Trim();
            var date = DateTime.ParseExact(columns[3].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var course = int.Parse(columns[4].Trim());
            var group = columns[5].Trim();
            var averageScore = float.Parse(columns[6].Trim());

            return new Student(lastname, firstname, patronomic, date, course, group, averageScore);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Ошибка парсинга данных студента: {ex.Message}", ex);
        }
    }

    public static Student CreateFromString(string data)
    {
        return Parse(data);
    }

    public override string ToString()
    {
        return $"Студент: {Lastname} {Name} {Patronomic}, Возраст: {Age}, Курс: {Course}, Группа: {Group}, Средний балл: {AverageScore:F2}";
    }
}

class Teacher : Person
{
    public string Department { get; }
    public int Experience { get; }
    public Position Position { get; }

    public Teacher(string lastName, string firstName, string middleName, DateTime date, string department, int experience, Position position) :
        base(lastName, firstName, middleName, date)
    {
        Department = department;
        Experience = experience;
        Position = position;
    }

    public static new Teacher Parse(string text)
    {
        var columns = text.Split([';'], StringSplitOptions.RemoveEmptyEntries);

        if (columns.Length != 7)
            throw new FormatException("Неверное количество колонок для преподавателя");

        try
        {
            var lastname = columns[0].Trim();
            var firstname = columns[1].Trim();
            var patronomic = columns[2].Trim();
            var date = DateTime.ParseExact(columns[3].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var departament = columns[4].Trim();
            var experience = int.Parse(columns[5].Trim());
            var position = (Position)Enum.Parse(typeof(Position), columns[6].Trim());

            return new Teacher(lastname, firstname, patronomic, date, departament, experience, position);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Ошибка парсинга данных преподавателя: {ex.Message}", ex);
        }
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
}

class University : IUniversity
{
    private List<IPerson> persons = new List<IPerson>();

    public IEnumerable<IPerson> Persons => persons.OrderBy(p => p.Date);
    public IEnumerable<Student> Students => persons.OfType<Student>().OrderBy(s => s.Date);
    public IEnumerable<Teacher> Teachers => persons.OfType<Teacher>().OrderBy(t => t.Date);

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
        if (string.IsNullOrWhiteSpace(lastName))
            return Enumerable.Empty<IPerson>();

        var searchName = lastName.Trim().ToLower();

        return persons.Where(p =>
            p.Lastname != null &&
            p.Lastname.Trim().ToLower().Contains(searchName));
    }

    public IEnumerable<Student> FindByAvrPoint(float avrPoint)
    {
        return persons.OfType<Student>()
                     .Where(s => s.AverageScore > avrPoint).OrderBy(s => s.AverageScore);
    }
}

class Program
{
    static University university = new University();

    static void Main(string[] args)
    {

        Console.OutputEncoding = Encoding.Unicode; 
        Console.InputEncoding = Encoding.Unicode;

        LoadDataFromFiles();

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
            Console.WriteLine("8. Удалить человека");
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


    static void LoadDataFromFiles()
    {
        try
        {
            string studentFile = "Data\\Student.txt";
            string teacherFile = "Data\\Teacher.txt";

            //Console.WriteLine($"Текущая директория: {Environment.CurrentDirectory}");
            //Console.WriteLine($"Путь к файлу студентов: {Path.GetFullPath(studentFile)}");

            if (File.Exists(studentFile))
            {
                LoadStudentsFromFile(studentFile);
                Console.WriteLine("Студенты загружены из файла Student.txt");
            }

            else {
                Console.WriteLine("Файл студентов не найден");
            }

            //Console.WriteLine($"Текущая директория: {Environment.CurrentDirectory}");
            //Console.WriteLine($"Путь к файлу преподавателей: {Path.GetFullPath(teacherFile)}");

            if (File.Exists(teacherFile))
            {
                LoadTeachersFromFile(teacherFile);
                Console.WriteLine("Преподаватели загружены из файла Teacher.txt");
            }
            else
            {
                Console.WriteLine("Файл преподавателей не найден");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
        }

        WaitForKey();   
    }


    static void LoadStudentsFromFile(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Файл {filename} не найден!");
            return;
        }

        var lines = File.ReadAllLines(filename, Encoding.UTF8);
        int loadedCount = 0;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var student = Student.Parse(line);
            university.Add(student);
            loadedCount++;
        }
    }

    static void LoadTeachersFromFile(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Файл {filename} не найден!");
            return;
        }

        var lines = File.ReadAllLines(filename, Encoding.UTF8);
        int loadedCount = 0;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var teacher = Teacher.Parse(line);
            university.Add(teacher);
            loadedCount++;
        }

    }

    static void DisplayPaginated<T>(IEnumerable<T> items, string title, Func<T, string> toString)
    {
        var itemList = items.ToList();

        if (!itemList.Any())
        {
            Console.WriteLine($"{title} не найдены.");
            WaitForKey();
            return;
        }

        int pageSize = 100;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(itemList.Count / (double)pageSize);

        while (currentPage < totalPages)
        {
            Console.Clear();
            Console.WriteLine($"{title} (стр. {currentPage + 1} из {totalPages}, всего: {itemList.Count})");
            Console.WriteLine("=".PadRight(80, '='));

            // Показываем записи текущей страницы
            var pageItems = itemList
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            for (int i = 0; i < pageItems.Count; i++)
            {
                int globalIndex = currentPage * pageSize + i;
                Console.WriteLine($"{globalIndex + 1}. {toString(pageItems[i])}");
            }

            // Меню навигации
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            if (totalPages > 1)
            {
                Console.WriteLine("Навигация: 'n' - след. страница, 'p' - пред. страница, 'q' - выход");
            }
            else
            {
                Console.WriteLine("'q' - выход");
            }

            Console.Write("Ваш выбор: ");
            var input = Console.ReadLine()?.ToLower();

            if (input == "n" && currentPage < totalPages - 1)
            {
                currentPage++;
            }
            else if (input == "p" && currentPage > 0)
            {
                currentPage--;
            }
            else if (input == "q")
            {
                break;
            }
            else if (!string.IsNullOrEmpty(input) && totalPages > 1)
            {
                Console.WriteLine("Неверный ввод! Используйте 'n', 'p' или 'q'.");
                WaitForKey();
            }
        }
    }


    static void InitializeTestData()
    {
        university.Add(new Student("Сидоров", "Иван", "Петрович", new DateTime(2000, 5, 15), 2, "ИСП-201", 4.2f));
        university.Add(new Student("Иванова", "Мария", "Сергеевна", new DateTime(2001, 8, 22), 1, "ИСП-101", 4.7f));
        university.Add(new Student("Петров", "Алексей", "Владимирович", new DateTime(1999, 3, 10), 3, "ИСП-301", 3.8f));

        university.Add(new Teacher("Smirnova", "Ольга", "Николаевна", new DateTime(1975, 12, 5), "Информационные системы", 15, Position.Professor));
        university.Add(new Teacher("Козлов", "Дмитрий", "Иванович", new DateTime(1980, 7, 18), "Программная инженерия", 10, Position.Docent));
        university.Add(new Teacher("Павлова", "Елена", "Викторовна", new DateTime(1985, 2, 28), "Информационные системы", 8, Position.SeniorLecturer));
    }

    static void ShowAllPersons()
    {
        DisplayPaginated(university.Persons.OrderBy(s => s.Lastname).ThenBy(s => s.Name),
            "=== Все люди в университете ===",
            p => p.ToString());
    }

    static void ShowAllStudents()
    {
        DisplayPaginated(university.Students.OrderBy(s => s.Lastname).ThenBy(s => s.Name),
            "=== Все студенты ===",
            s => s.ToString());
    }

    static void ShowAllTeachers()
    {
        DisplayPaginated(university.Teachers.OrderBy(s => s.Lastname).ThenBy(s => s.Name),
            "=== Все преподаватели ===",
            t => t.ToString());
    }


    static void AddTeacher()
    {
        try
        {
            Console.WriteLine("\n=== Добавление преподавателя ===");
            Console.WriteLine("Введите данные в формате: Фамилия; Имя; Отчество; ДатаРождения(дд.мм.гггг); Кафедра; Стаж; Должность");
            Console.WriteLine("Должности: Assistant, SeniorLecturer, Docent, Professor, DepartmentHead");
            Console.WriteLine("Пример: Петров; Иван; Михайлович; 20.08.1975; Информационные системы; 15; Professor");
            Console.Write("Данные: ");
            var input = Console.ReadLine();

            var teacher = Teacher.Parse(input);
            university.Add(teacher);
            Console.WriteLine("Преподаватель успешно добавлен!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        WaitForKey();
    }

    static void AddStudent()
    {
        try
        {
            Console.WriteLine("\n=== Добавление студента ===");
            Console.WriteLine("Введите данные в формате: Фамилия; Имя; Отчество; ДатаРождения(дд.мм.гггг); Курс; Группа; СреднийБалл");
            Console.WriteLine("Пример: Иванов; Петр; Сергеевич; 15.05.2000; 2; ИСП-201; 4,5");
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

    static void FindByLastName()
    {
        Console.WriteLine("\n=== Поиск по фамилии ===");
        Console.Write("Введите фамилию: ");
        var lastName = Console.ReadLine();

        var results = university.FindByLastName(lastName);

        DisplayPaginated(results,
            $"=== Результаты поиска по фамилии '{lastName}' ===",
            p => p.ToString());
    }

    static void FindByAvrPoint()
    {
        Console.WriteLine("\n=== Поиск студентов с баллом выше заданного ===");
        Console.Write("Введите минимальный средний балл: ");

        if (float.TryParse(Console.ReadLine(), out float avrPoint))
        {
            var results = university.FindByAvrPoint(avrPoint);

            DisplayPaginated(results,
                $"=== Студенты со средним баллом выше {avrPoint:F2} ===",
                s => s.ToString());
        }
        else
        {
            Console.WriteLine("Неверный формат балла!");
            WaitForKey();
        }
    }


    static void RemovePerson()
    {
        Console.WriteLine("\n=== Удаление человека ===");
        Console.Write("Введите фамилию: ");
        var lastName = Console.ReadLine();

        var results = university.FindByLastName(lastName).OrderBy(p => p.Name).ToList();

        if (results.Any())
        {
            Console.WriteLine($"Найдено {results.Count()} записей:");

            int pageSize = 100;
            int currentPage = 0;
            int totalPages = (int)Math.Ceiling(results.Count / (double)pageSize);

            while (currentPage < totalPages)
            {
                Console.WriteLine($"\n--- Страница {currentPage + 1} из {totalPages} ---");

                // Показываем записи текущей страницы
                var pageItems = results
                    .Skip(currentPage * pageSize)
                    .Take(pageSize)
                    .ToList();

                for (int i = 0; i < pageItems.Count; i++)
                {
                    int globalIndex = currentPage * pageSize + i;
                    Console.WriteLine($"{globalIndex + 1}. {pageItems[i]}");
                }

                // Меню навигации
                if (currentPage < totalPages - 1)
                {
                    Console.WriteLine("\n'n' - следующая страница, 'p' - предыдущая страница, 'число' - выбрать для удаления, 'q' - отмена");
                }
                else
                {
                    Console.WriteLine("\n'p' - предыдущая страница, 'число' - выбрать для удаления, 'q' - отмена");
                }

                Console.Write("Ваш выбор: ");
                var input = Console.ReadLine()?.ToLower();

                if (input == "n" && currentPage < totalPages - 1)
                {
                    currentPage++;
                    Console.Clear();
                    Console.WriteLine($"=== Удаление человека (найдено {results.Count()} записей) ===");
                }
                else if (input == "p" && currentPage > 0)
                {
                    currentPage--;
                    Console.Clear();
                    Console.WriteLine($"=== Удаление человека (найдено {results.Count()} записей) ===");
                }
                else if (input == "q")
                {
                    Console.WriteLine("Операция отменена.");
                    return;
                }
                else if (int.TryParse(input, out int selectedIndex) &&
                         selectedIndex >= 1 && selectedIndex <= results.Count)
                {
                    // Удаление выбранной записи
                    var personToRemove = results[selectedIndex - 1];
                    university.Remove(personToRemove);
                    Console.WriteLine($"Человек успешно удален: {personToRemove.Lastname} {personToRemove.Name}");
                    WaitForKey();
                    return;
                }
                else
                {
                    Console.WriteLine("Неверный ввод! Попробуйте снова.");
                    WaitForKey();
                    Console.Clear();
                    Console.WriteLine($"=== Удаление человека (найдено {results.Count()} записей) ===");
                }
            }
        }
        else
        {
            Console.WriteLine("Люди с такой фамилией не найдены.");
        }

        WaitForKey();
    }

    static void WaitForKey()
    {
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }
}