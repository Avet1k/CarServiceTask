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
    private List<Part> _parts;

    public Car()
    {
        string[] partNames = PartNames.GetNames();
        
        _parts = new List<Part>();

        for (int i = 0; i < partNames.Length; i++)
            _parts.Add(new Part(partNames[i]));

        BreakRandomPart();
    }

    public int GetPartsCount => _parts.Count;

    public Part GetPartByIndex(int index)
    {
        return _parts[index];
    }

    private void BreakRandomPart()
    {
        Random random = new Random();
        int partIndex = random.Next(GetPartsCount);
        
        _parts[partIndex].Break();
    }

    public bool TrySwapParts(Part newPart, out Part brokenPart)
    {
        foreach (Part part in _parts)
        {
            if (part.Name == newPart.Name)
            {
                brokenPart = part;
                _parts.Remove(part);
                _parts.Add(newPart);
                return true;
            }
        }

        brokenPart = null;
        return false;
    }
}

class CarService
{
    private int _money;
    private int _forfeit;
    private Storage _storage;
    private Dictionary<string, int> _prices;

    public CarService()
    {
        Random random = new Random();
        int minPrice = 200;
        int maxPrice = 1000;
        
        _storage = new Storage();
        _forfeit = 100;
        _prices = new Dictionary<string, int>();

        foreach (string partName in PartNames.GetNames())
            _prices.Add(partName, random.Next(minPrice, maxPrice));
    }

    public void Work()
    {
        const char ExitCommand = 'q';
        
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

            if (_storage.TryGetPart(partNameNeeded, out Part part))
            {
                car.TrySwapParts(part, out Part brokenPart);
                Summarize(brokenPart);
            }
            else if (partNameNeeded != string.Empty)
            {
                Console.WriteLine($"\nНа складе нет детали {partNameNeeded}");
                PayForeit();
            }
            
            Console.Write($"\nНажмите любую кнопку для продолжения или {ExitCommand} для выхода из программы...");

            if (Console.ReadKey(true).KeyChar == ExitCommand)
                isWorking = false;
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
        const string Refusial = "r";
        
        string userInput;
        string partNameNeeded = string.Empty;
        List<string> partNames = _storage.GetPartNames();
        bool isInputCorrect = false;
        
        ShowRapairOptions(partNames);

        while (isInputCorrect == false)
        {
            Console.Write($"\nЧтобы отказать клиенту, введите '{Refusial}'\n" +
                          "Введите номер детали: ");
            userInput = Console.ReadLine();

            if (userInput == Refusial)
            {
                PayForeit();
                isInputCorrect = true;
            }
            else if (int.TryParse(userInput, out int partNameNumber)
                     && partNameNumber > 0
                     && partNameNumber <= partNames.Count)
            {
                partNameNeeded = partNames[partNameNumber - 1];
                isInputCorrect = true;
            }
            else
            {
                Console.WriteLine("Ошибка ввода!");
            }
        }

        return partNameNeeded;
    }

    private void ShowRapairOptions(List<string> partNames)
    {
         Console.WriteLine("\n   Выберите деталь для замены:");
                
         for (int i = 0; i < partNames.Count; i++)
         {
             string partName = partNames[i];
             int number = i + 1;
             int partsAmount = _storage.GetPartAmount(partNames[i]);
                     
             Console.WriteLine($"{number}. {partName}: {partsAmount} шт");
         }
    }

    private void ShowInfo()
    {
        Console.WriteLine($"Касса: {_money}₮");
    }

    private void PayForeit()
    {
        _money -= _forfeit;
        Console.WriteLine($"\nВы отказали клиенту и оплатили штраф {_forfeit}₮.");
    }

    private void Summarize(Part brokenPart)
    {
        int price = _prices[brokenPart.Name];

        if (brokenPart.IsBroken)
        {
            _money += price;
            Console.WriteLine($"\nВы успешно устранили поломку: {brokenPart.Name}." +
                              $"\nКасса увеличилась на {price}₮ за выполненную работу.");
        }
        else 
        { 
            _money -= price; 
            Console.WriteLine($"\nВы не устранили поломку, а поменяли целые детали местами: {brokenPart.Name}." + 
                              $"\nКасса уменьшилась на {price}₮ за возмещение ущерба.");
        }
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

    public int GetPartAmount(string name)
    {
        return _parts[name].Count;
    }
}