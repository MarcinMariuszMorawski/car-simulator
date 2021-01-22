﻿using CarSimulatorEngine.Enums;
using CarSimulatorEngine.Exceptions;
using CarSimulatorUserInterfaceConsole.Extensions;
using CarSimulatorUserInterfaceConsole.Model;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarSimulatorUserInterfaceConsole.UserInterface
{
    public class CarSimulatorUserInterfaceConsole
    {
        private string LastToUserMessage { get; set; } = "";
        private int ConsoleHeight { get; set; } = Console.WindowHeight;
        private int ConsoleWidth { get; set; } = Console.WindowWidth;
        private bool IsExitEventTriggered { get; set; } = false;
        private CarSimulatorEngine.Engine.CarSimulatorEngine CarSimulatorEngine { get; set; }

        public async Task Work()
        {
            PrepareConsole();
            await Loading();
            InitEngine();
            WorkingThread();
            ControlLoop();
        }

        private async Task Loading()
        {
            Console.Clear();
            var rand = new Random();

            const string text = "Please maximize console window ... and press key";

            Console.SetCursorPosition(Console.WindowWidth / 2 - text.Length / 2, Console.WindowHeight / 2 - 1);
            Console.Write(text.Pastel(Color.Red));
            while (!Console.KeyAvailable)
            {
            }

            PrepareConsole();
            Console.SetCursorPosition(0, Console.WindowHeight / 2);
            for (var i = 1; i <= Console.WindowWidth; i++)
            {
                Console.Write($" ".PastelBg(Color.Red));
                await Task.Delay(rand.Next(1, 30));
            }

            Console.Clear();
        }

        private void ControlLoop()
        {
            while (true)
            {
                if (IsExitEventTriggered)
                {
                    break;
                }

                if (!Console.KeyAvailable)
                {
                    continue;
                }

                KeyToSwitch();
            }
        }

        private void WorkingThread()
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;

                try
                {
                    while (true)
                    {
                        if (IsExitEventTriggered)
                        {
                            break;
                        }

                        CarSimulatorEngine.Work();
                        DrawUserInterface();
                        await Task.Delay(1000);
                    }
                }
                catch (CarSimulatorException exception)
                {
                    LastToUserMessage = exception.Message;
                }
            }).Start();
        }

        private void KeyToSwitch()
        {
            var key = Console.ReadKey(true).Key;

            try
            {
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        CarSimulatorEngine.Accelerate();
                        break;
                    case ConsoleKey.DownArrow:
                        CarSimulatorEngine.Decelerate();
                        break;
                    case ConsoleKey.W:
                        CarSimulatorEngine.GearUp();
                        break;
                    case ConsoleKey.S:
                        CarSimulatorEngine.GearDown();
                        break;
                    case ConsoleKey.D1:
                        CarSimulatorEngine.StartCarEngine();
                        break;
                    case ConsoleKey.D2:
                        CarSimulatorEngine.StopCarEngine();
                        break;
                    case ConsoleKey.F:
                        CarSimulatorEngine.FillFuelTank();
                        break;
                    case ConsoleKey.Escape:
                        IsExitEventTriggered = true;
                        break;
                }
            }
            catch (CarSimulatorException exception)
            {
                LastToUserMessage = exception.Message;
            }
        }

        private void DrawUserInterface()
        {
            var mainSplitBasicBoxes = GetMainSplitBasicBoxes();
            var messageToDriverBasicBoxes = GetMessageToDriverBasicBoxes();
            var carFaultsBasicBoxes = GetCarFaultsBasicBoxes();

            var topPosition = 0;
            var leftPosition = 1;

            CheckIfConsoleSizeChangedAndClear();

            if (mainSplitBasicBoxes.Any())
            {
                topPosition = DrawBasicBoxes(leftPosition, topPosition + 2, mainSplitBasicBoxes, Color.Orange,
                    Color.Aqua);
            }

            if (messageToDriverBasicBoxes.Any())
            {
                topPosition = DrawBasicBoxes(leftPosition, topPosition + 2, messageToDriverBasicBoxes, Color.Blue,
                    Color.Aqua);
            }

            if (carFaultsBasicBoxes.Any())
            {
                DrawBasicBoxes(leftPosition, topPosition + 2, carFaultsBasicBoxes, Color.Blue,
                    Color.Aqua);
            }
        }

        private List<BasicBox> GetMessageToDriverBasicBoxes()
        {
            var title = "Last message to driver:";
            return new List<BasicBox>
            {
                new BasicBox($"{title}{" ".MultiplyString(Console.WindowWidth - title.Length - 4)}", LastToUserMessage)
            };
        }

        private List<BasicBox> GetCarFaultsBasicBoxes()
        {
            return CarSimulatorEngine.CarFaults.Select(x => new BasicBox("Fault", x.ToString())).ToList();
        }

        private List<BasicBox> GetMainSplitBasicBoxes()
        {
            var keysBasicBoxes = GetKeyBasicBoxes();
            var carBasicBoxes = GetCarBasicBoxes();
            var splitMainBasicBoxes = new List<BasicBox>(keysBasicBoxes);
            splitMainBasicBoxes.AddRange(carBasicBoxes);
            return splitMainBasicBoxes;
        }

        private IEnumerable<BasicBox> GetCarBasicBoxes()
        {
            var carBasicBoxes = new List<BasicBox>
            {
                new BasicBox("Car state", $"{CarSimulatorEngine.CarState}",
                    CarSimulatorEngine.CarState == CarStates.Off ? Color.Red : Color.Green, Color.Aqua),

                new BasicBox("Gear", $"{CarSimulatorEngine.UsedGear}", Color.DodgerBlue, Color.Aqua),
                new BasicBox("Gear max ", $"{CarSimulatorEngine.MaxGear}", Color.DodgerBlue, Color.Aqua),
                new BasicBox("Gear min", $"{CarSimulatorEngine.MinGear}", Color.DodgerBlue, Color.Aqua),

                new BasicBox("Fuel", $"{CarSimulatorEngine.Fuel}", Color.DarkSlateBlue, Color.Aqua),
                new BasicBox("Fuel capacity", $"{CarSimulatorEngine.FuelCapacity}", Color.DarkSlateBlue, Color.Aqua),
                new BasicBox("Fuel consumption", $"{CarSimulatorEngine.FuelConsumption}", Color.DarkSlateBlue,
                    Color.Aqua),

                new BasicBox("Engine speed", $"{CarSimulatorEngine.EngineSpeed}", Color.IndianRed, Color.Aqua),
                new BasicBox("Engine max speed", $"{CarSimulatorEngine.EngineSpeedMaxValue}", Color.IndianRed,
                    Color.Aqua),

                new BasicBox("Car speed", $"{CarSimulatorEngine.Speed}", Color.DarkGoldenrod, Color.Aqua),

                new BasicBox("Oil value", $"{CarSimulatorEngine.EngineOil}", Color.Gray, Color.Aqua),
                new BasicBox("Oil min good value", $"{CarSimulatorEngine.EngineOilGoodMinValue}", Color.Gray,
                    Color.Aqua),
                new BasicBox("Oil max good valued", $"{CarSimulatorEngine.EngineOilGoodMaxValue}", Color.Gray,
                    Color.Aqua),
            };
            return carBasicBoxes;
        }

        private static IEnumerable<BasicBox> GetKeyBasicBoxes()
        {
            var keysBasicBoxes = new List<BasicBox>
            {
                new BasicBox("Start car engine", "1"),
                new BasicBox("Stop car engine", "2"),
                new BasicBox("Accelerate", "Up Arrow"),
                new BasicBox("Decelerate", "Down Arrow"),
                new BasicBox("Gear up", "W"),
                new BasicBox("Gear down", "S"),
                new BasicBox("Fill fuel tank", "F"),
                new BasicBox("Shut down game", "Esc"),
            };
            return keysBasicBoxes.Select(x => new BasicBox(x.Title, x.Value, Color.Blue, Color.Aqua));
        }

        private void CheckIfConsoleSizeChangedAndClear()
        {
            if (ConsoleHeight != Console.WindowHeight || ConsoleWidth != Console.WindowWidth)
            {
                ConsoleHeight = Console.WindowHeight;
                ConsoleWidth = Console.WindowWidth;
                Console.Clear();
                Console.CursorVisible = false;
            }
        }

        private int DrawBasicBoxes(int left, int top, IReadOnlyCollection<BasicBox> basicBoxes, Color backgroundColor,
            Color textColor)
        {
            Console.SetCursorPosition(left, top);
            var startingLeftValue = left;
            var boxWidth = basicBoxes.Max(x => Math.Max(x.Value.Length, x.Title.Length)) + 2;
            var consoleWidth = Console.WindowWidth;
            foreach (var basicBox in basicBoxes)
            {
                var backgroundColorToUse =
                    basicBox.BackgroundColor == Color.Empty ? backgroundColor : basicBox.BackgroundColor;
                var textColorToUse = basicBox.TextColor == Color.Empty ? textColor : basicBox.TextColor;

                if (left + boxWidth > consoleWidth)
                {
                    top += 6;
                    left = startingLeftValue;
                }

                Console.SetCursorPosition(left, top);
                Console.WriteLine(" ".MultiplyString(boxWidth).PastelBg(backgroundColorToUse));
                Console.SetCursorPosition(left, top + 1);
                Console.WriteLine(" ".MultiplyString(boxWidth).PastelBg(backgroundColorToUse));
                Console.SetCursorPosition(left, top + 2);
                Console.WriteLine(" ".MultiplyString(boxWidth).PastelBg(backgroundColorToUse));
                Console.SetCursorPosition(left, top + 3);
                Console.WriteLine(" ".MultiplyString(boxWidth).PastelBg(backgroundColorToUse));

                Console.SetCursorPosition(left, top + 1);
                Console.WriteLine($"{" ".MultiplyString((boxWidth - basicBox.Title.Length) / 2)}{basicBox.Title}"
                    .Pastel(textColorToUse).PastelBg(backgroundColorToUse));
                Console.SetCursorPosition(left, top + 2);
                Console.WriteLine($"{" ".MultiplyString((boxWidth - basicBox.Value.Length) / 2)}{basicBox.Value}"
                    .Pastel(textColorToUse).PastelBg(backgroundColorToUse));

                left += boxWidth + 5;
            }

            return top + 3;
        }

        private void InitEngine()
        {
            CarSimulatorEngine = new CarSimulatorEngine.Engine.CarSimulatorEngine(CarTypes.Truck);
        }

        private static void PrepareConsole()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
        }
    }
}