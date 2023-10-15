using System.Collections.Specialized;

namespace CarServiceTask;
class Program
{
    static void Main(string[] args)
    {
        CarService service = new CarService();
        
        service.Work();
    }
}

public class PartNames
{
    public const string Engine = "Двигатель";
    public const string Transmission = "Трансмиссия";
    public const string Suspension = "Подвеска";
    public const string Steering = "Рулевое";
    public const string Wheels = "Колёса";
    public const string Brakes = "Тормоза";
    public const string Electronics = "Электроника";

    public static string[] GetNames()
    {
        return new[] { Engine, Transmission, Suspension, Steering, Wheels, Brakes, Electronics };
    }
}

class Part
{
    public Part(string name)
    {
        Name = name;
        IsBroken = false;
    }

    public string Name { get; private set; }
    public bool IsBroken { get; private set; }

    public void Break()
    {
        IsBroken = true;
    }
}

class Car
{
    private Part[] _parts;

    public Car()
    {
        string[] partNames = PartNames.GetNames();
        
        _parts = new Part[partNames.Length];

        for (int i = 0; i < partNames.Length; i++)
            _parts[i] = new Part(partNames[i]);

        BreakRandomPart();
    }

    public int GetPartsCount => _parts.Length;

    public Part GetPartByIndex(int index)
    {
        return _parts[index];
    }

    private void BreakRandomPart()
    {
        Random random = new Random();
        int partIndex = random.Next(_parts.Length);
        
        _parts[partIndex].Break();
    }
}

class CarService
{
    private int _money;
    private Storage _storage;

    public CarService()
    {
        _storage = new Storage();
    }

    public void Work()
    {
        bool isWorking = true;

        while (isWorking)
        {
            Car car = new Car();
            string partNameNeeded;

            Console.Clear();
            ShowInfo();
            Console.WriteLine("Эвакуатор привёз машину с поломкой.\n");
            
            Analyze(car);

            partNameNeeded = HandlePartNameInput();
            
            if (isWorking == false)
                continue;

            Console.Write("\nНажмите любую кнопку для продолжения...");
            Console.ReadKey();
        }
    }

    private void Analyze(Car car)
    {
        char okSymbol = '☑';
        char notOkSymbol = '☒';

        Console.WriteLine("  Диагностика:");
        for (int i = 0; i < car.GetPartsCount; i++)
        {
            char status = okSymbol;
            Part part = car.GetPartByIndex(i);

            if (part.IsBroken)
                status = notOkSymbol;
            
            Console.WriteLine($"{status} {part.Name}");
        }
    }
    
    private string HandlePartNameInput()
    {
        string userInput;
        string partNameNeeded = string.Empty;
        List<string> partNames = _storage.GetPartNames();
        bool isInputCorrect = false;

        for (int i = 0; i < partNames.Count; i++)
        {
            string partName = partNames[i];
            int number = i + 1;
            int partsAmount = _storage.GetPartsAmount(partNames[i]);
            
            Console.WriteLine($"{number}. {partName}: {partsAmount}");
        }

        while (isInputCorrect == false)
        {
            Console.Write("Введите номер детали: ");
            userInput = Console.ReadLine();

            if (int.TryParse(userInput, out int partNameNumber)
                && partNameNumber > 0
                && partNameNumber < partNames.Count)
            {
                partNameNeeded = partNames[partNameNumber];
                isInputCorrect = true;
            }
            else
            {
                Console.WriteLine("Ошибка ввода, нужно внести номер детали!");
            }
        }

        return partNameNeeded;
    }
    
    private void ShowInfo()
    {
        Console.WriteLine($"Касса: {_money}₮");
    }
}

class Storage
{
    private Dictionary<string, Queue<Part>> _parts;

    public Storage()
    {
        Random random = new Random();
        string[] partNames = PartNames.GetNames();
        int maxAmount = 10;

        _parts = new Dictionary<string, Queue<Part>>();

        foreach (string partName in partNames)
        {
            int partAmount = random.Next(maxAmount);
            
            _parts.Add(partName, new Queue<Part>());
            
            for (int i = 0; i < partAmount; i++)
                _parts[partName].Enqueue(new Part(partName));
        }
    }

    public bool TryGetPart(string name, out Part part)
    {
        if (_parts.ContainsKey(name) && _parts[name].Count > 0)
        {
            part = _parts[name].Dequeue();
            return true;
        }
        
        part = null;
        return false;
    }

    public List<string> GetPartNames()
    {
        List<string> partNames = new List<string>();

        foreach (string key in _parts.Keys)
            partNames.Add(key);

        return partNames;
    }

    public int GetPartsAmount(string name)
    {
        return _parts[name].Count;
    }
}