using System;
using System.Management.Automation.Host;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    // With a little help from:
    //    https://msdn.microsoft.com/en-us/library/ee706570(v=vs.85).aspx

    public class Ps5CustomHostRawUI : PSHostRawUserInterface
    {
        private ILogger _logger;

        public Ps5CustomHostRawUI(ILogger logger)
        {
            _logger = logger;
        }

        public override ConsoleColor BackgroundColor
        {
            get
            {
                _logger.LogWarning("IGNORED: get_" + nameof(BackgroundColor));
                return Console.BackgroundColor;
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(BackgroundColor));
                throw new NotImplementedException();
            }
        }

        public override Size BufferSize
        {
            get
            {
                return new Size(Console.BufferWidth, Console.BufferHeight);
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(BufferSize));
                throw new NotImplementedException();
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                _logger.LogError("NOT IMPLEMENTED: get_" + nameof(CursorPosition));
                throw new NotImplementedException();
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(CursorPosition));
                throw new NotImplementedException();
            }
        }

        public override int CursorSize
        {
            get
            {
                return Console.CursorSize;
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(CursorSize));
                throw new NotImplementedException();
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                _logger.LogWarning("IGNORED: get_" + nameof(ForegroundColor));
                return Console.ForegroundColor;
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(ForegroundColor));
                throw new NotImplementedException();
            }
        }

        public override bool KeyAvailable
        {
            get
            {
                return false;
            }
        }

        public override Size MaxPhysicalWindowSize
        {
            get
            {
                return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);
            }
        }

        public override Size MaxWindowSize
        {
            get
            {
                return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);
            }
        }

        public override Coordinates WindowPosition
        {
            get
            {
                return new Coordinates(Console.WindowLeft, Console.WindowTop);
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(WindowPosition));
                throw new NotImplementedException();
            }
        }

        public override Size WindowSize
        {
            get
            {
                return new Size(Console.WindowWidth, Console.WindowHeight);
            }

            set
            {
                _logger.LogError("NOT IMPLEMENTED: set_" + nameof(WindowSize));
                throw new NotImplementedException();
            }
        }

        public override string WindowTitle
        { get; set; }

        public override void FlushInputBuffer()
        { }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(GetBufferContents));
            throw new NotImplementedException();
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(ReadKey));
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(ScrollBufferContents));
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(SetBufferContents));
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(SetBufferContents));
            throw new NotImplementedException();
        }
    }
}