using System;
using NAudio.Wave;

class Program
{
    private static WaveOutEvent backgroundPlayer;
    private static AudioFileReader backgroundReader;

    static void Main(string[] args)
    {
        Random random = new Random();
        int balance = 1000;

        // Запуск фоновой музыки
        StartBackgroundMusic("casino_background.mp3");

        Console.WriteLine("Добро пожаловать в казино!");

        try
        {
            while (balance > 0)
            {
                Console.WriteLine($"Ваш баланс: {balance} руб.");
                Console.WriteLine("Введите вашу ставку (или 0 для выхода):");

                string input = Console.ReadLine();
                int bet;

                if (!int.TryParse(input, out bet))
                {
                    Console.WriteLine("Ошибка: введите корректное число.");
                    continue;
                }

                if (bet < 0)
                {
                    Console.WriteLine("Ставка не может быть отрицательной.");
                    continue;
                }

                if (bet == 0)
                {
                    while (true)
                    {
                        Console.WriteLine("Вы действительно хотите выйти из игры? (Y/N)");
                        string answer = Console.ReadLine();

                        if (answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Game Over. Спасибо за игру!");
                            StopBackgroundMusic();
                            PauseBeforeExit();
                            return;
                        }
                        else if (answer.Equals("N", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Продолжаем игру.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Пожалуйста, введите Y (да) или N (нет).");
                        }
                    }
                    continue;
                }

                if (bet > balance)
                {
                    Console.WriteLine("Недостаточно средств для этой ставки.");
                    continue;
                }

                // Динамическое управление шансами
                int maxSlotValue = balance < 300_000 ? 4 : 10;

                int slot1 = random.Next(1, maxSlotValue + 1);
                int slot2 = random.Next(1, maxSlotValue + 1);
                int slot3 = random.Next(1, maxSlotValue + 1);

                Console.WriteLine($"Результат: {slot1} {slot2} {slot3}");

                bool isTriple = (slot1 == slot2 && slot2 == slot3);
                bool isPair = (slot1 == slot2 || slot2 == slot3 || slot1 == slot3);

                if (isTriple)
                {
                    int winnings = bet * 10;
                    balance += winnings;
                    Console.WriteLine($"Поздравляем! Вы выиграли {winnings} руб. (Тройка!)");
                }
                else if (isPair)
                {
                    int winnings = bet * 3;
                    balance += winnings;
                    Console.WriteLine($"Поздравляем! Вы выиграли {winnings} руб. (Пара!)");
                }
                else
                {
                    balance -= bet;
                    Console.WriteLine("Вы проиграли.");
                }
            }

            Console.WriteLine("У вас закончились деньги. Game Over.");
            StopBackgroundMusic();
            PauseBeforeExit();
        }
        finally
        {
            // Гарантированное завершение фоновой музыки при любом выходе
            StopBackgroundMusic();
        }
    }

    static void StartBackgroundMusic(string filePath)
    {
        try
        {
            backgroundReader = new AudioFileReader(filePath);
            backgroundPlayer = new WaveOutEvent();
            var loop = new LoopStream(backgroundReader); // Зацикливание музыки
            backgroundPlayer.Init(loop);
            backgroundPlayer.Play();
        }
        catch (Exception)
        {
            Console.WriteLine("Не удалось воспроизвести фоновую музыку. Проверьте наличие файла casino_background.mp3.");
        }
    }

    static void StopBackgroundMusic()
    {
        try
        {
            if (backgroundPlayer != null)
            {
                backgroundPlayer.Stop();
                backgroundPlayer.Dispose();
            }
            if (backgroundReader != null)
            {
                backgroundReader.Dispose();
            }
        }
        catch (Exception)
        {
            // Игнорируем ошибки при остановке
        }
    }

    static void PauseBeforeExit()
    {
        Console.WriteLine("Нажмите любую клавишу, чтобы выйти...");
        Console.ReadKey(true);
    }

    // Класс для зацикливания фоновой музыки
    private class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.sourceStream.Position = 0;
        }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    sourceStream.Position = 0; // Перемотка в начало для зацикливания
                }
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sourceStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}