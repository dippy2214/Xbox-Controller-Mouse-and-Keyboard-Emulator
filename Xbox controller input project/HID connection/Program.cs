// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;
using Input;


public class HIDConnection
{
    //define button bitmasks
    public const uint XINPUT_GAMEPAD_A = 0x1000;
    public const uint XINPUT_GAMEPAD_B = 0x2000;
    public const uint XINPUT_GAMEPAD_X = 0x4000;
    public const uint XINPUT_GAMEPAD_Y = 0x8000;

    public const uint XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
    public const uint XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;

    public const uint XINPUT_GAMEPAD_START = 0x0010;
    public const uint XINPUT_GAMEPAD_BACK = 0x0020;
    public const uint XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
    public const uint XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080;

    public const uint XINPUT_GAMEPAD_DPAD_UP = 0x0001;
    public const uint XINPUT_GAMEPAD_DPAD_DOWN = 0x0002;
    public const uint XINPUT_GAMEPAD_DPAD_LEFT = 0x0004;
    public const uint XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008;

    [StructLayout(LayoutKind.Sequential)]
    struct XINPUT_STATE
    {
        public uint dwPackageNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XINPUT_GAMEPAD
    {
        public ushort wButtons; // Bitmask for buttons (A, B, X, Y, LB, RB, etc.)
        public byte bLeftTrigger; // Left trigger value (0-255)
        public byte bRightTrigger; // Right trigger value (0-255)
        public short sThumbLX; // Left thumbstick X-axis value (-32768 to 32767)
        public short sThumbLY; // Left thumbstick Y-axis value (-32768 to 32767)
        public short sThumbRX; // Right thumbstick X-axis value (-32768 to 32767)
        public short sThumbRY; // Right thumbstick Y-axis value (-32768 to 32767)
    }

    [StructLayout(LayoutKind.Sequential)]
    struct EMULATED_INPUTS
    {
        //A is on left click
        public InputKeys B;
        //X is right click
        public InputKeys Y;

        //start switches to custom button map
        //back begins binding process for a single button
        public InputKeys Left_Stick_Press;
        public InputKeys Right_Stick_Press;

        public InputKeys Dpad_Up;
        public InputKeys Dpad_Down;
        public InputKeys Dpad_Left;
        public InputKeys Dpad_Right;

        public InputKeys Left_Trigger;
        public InputKeys Right_Trigger;

        public InputKeys Right_Stick_Left;
        public InputKeys Right_Stick_Right;
        public InputKeys Right_Stick_Up;
        public InputKeys Right_Stick_Down;

        public EMULATED_INPUTS()
        {
            B = InputKeys.Escape;
            Y = InputKeys.Enter;
            Dpad_Up = InputKeys.Up;
            Dpad_Down = InputKeys.Down;
            Dpad_Left = InputKeys.Left;
            Dpad_Right = InputKeys.Right;
            Right_Stick_Down = InputKeys.S;
            Right_Stick_Up = InputKeys.W;
            Right_Stick_Left = InputKeys.A;
            Right_Stick_Right = InputKeys.D;
        }
    }

    static bool customControls = false;
    static bool amBinding = false;
    
    static EMULATED_INPUTS defaultButtonMap = new EMULATED_INPUTS();
    static EMULATED_INPUTS customButtonMap = new EMULATED_INPUTS();

    [DllImport("xinput1_4.dll")]
    static extern uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState);

    static void Main()
    {
        IKeyboardSimulation keyboardSim = Inputs.Use<IKeyboardSimulation>();
        IMouseSimulation mouseSim = Inputs.Use<IMouseSimulation>();
        

        const int targetFPS = 144;
        int frameTime = (int)(1000.0f/targetFPS);


        XINPUT_STATE state = new XINPUT_STATE();
        uint controllerIndex = 0;


        Console.WriteLine("Xbox Controller -> Mouse and Keyboard Input Emulator");

        Console.WriteLine("Standard Layout");
        Console.WriteLine("A = Left Click");
        Console.WriteLine("X = Right Click");
        Console.WriteLine("B = Escape");
        Console.WriteLine("Y = Enter");
        Console.WriteLine("");

        Console.WriteLine("Dpad up = Arrow key up");
        Console.WriteLine("Dpad down = Arrow key down");
        Console.WriteLine("Dpad left = Arrow key left");
        Console.WriteLine("Dpad right = Arrow key right");
        Console.WriteLine("");

        Console.WriteLine("Left stick = Mouse");
        Console.WriteLine("Right Bumper = Mouse speed increase");
        Console.WriteLine("Left Bumper = Mouse speed decrease");

        //Console.WriteLine("");
        //Console.WriteLine("Start = switch to custom button map");
        //Console.WriteLine("Back = bind button");
        //Console.WriteLine("");
        

        while (true)
        {
            if (!customControls)
            {
                UpdateInput(keyboardSim, mouseSim, controllerIndex, state, defaultButtonMap);
            }
            else
            {
                UpdateInput(keyboardSim, mouseSim, controllerIndex, state, customButtonMap);
            }

            Thread.Sleep(frameTime);
        }
    }

    static void UpdateInput(IKeyboardSimulation keyboardSim, IMouseSimulation mouseSim, uint controllerIndex, XINPUT_STATE state, EMULATED_INPUTS buttonMap)
    {
        uint result = XInputGetState(controllerIndex, ref state);

        int mouseMoveSpeed = 4;
        

        if (result == 0) // Success
        {
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_A) != 0)
            {
                mouseSim.Click();
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_B) != 0)
            {
                keyboardSim.KeyClick(buttonMap.B);
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_X) != 0)
            {
                mouseSim.RightClick();
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_Y) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Y);
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_SHOULDER) != 0)
            {
                mouseMoveSpeed = 1;
            }
            else if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0)
            {
                mouseMoveSpeed = 5;
            }
            else
            {
                mouseMoveSpeed = 3;
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_START) != 0)
            {
                customControls = true;
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_BACK) != 0)
            {
                //binding?
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_THUMB) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Left_Stick_Press);
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_THUMB) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Right_Stick_Press);
            }

            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Dpad_Up);
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Dpad_Down);
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Dpad_Left);
            }
            if ((state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0)
            {
                keyboardSim.KeyClick(buttonMap.Dpad_Right);
            }

            if (state.Gamepad.bLeftTrigger >= 100)
            {
                keyboardSim.KeyClick(buttonMap.Left_Trigger);
            }
            if (state.Gamepad.bRightTrigger >= 100)
            {
                keyboardSim.KeyClick(buttonMap.Right_Trigger);
            }


            if (state.Gamepad.sThumbLY >= 10000)
            {
                mouseSim.Move(0, -mouseMoveSpeed);
            }
            if (state.Gamepad.sThumbLY <= -10000)
            {
                mouseSim.Move(0, mouseMoveSpeed);
            }
            if (state.Gamepad.sThumbLX >= 10000)
            {
                mouseSim.Move(mouseMoveSpeed, 0);
            }
            if (state.Gamepad.sThumbLX <= -10000)
            {
                mouseSim.Move(-mouseMoveSpeed, 0);
            }

            if (state.Gamepad.sThumbRY >= 10000)
            {
                keyboardSim.KeyClick(buttonMap.Right_Stick_Up);
            }
            if (state.Gamepad.sThumbRY <= -10000)
            {
                keyboardSim.KeyClick(buttonMap.Right_Stick_Down);
            }
            if (state.Gamepad.sThumbRX >= 10000)
            {
                keyboardSim.KeyClick(buttonMap.Right_Stick_Left);
            }
            if (state.Gamepad.sThumbRX <= -10000)
            {
                keyboardSim.KeyClick(buttonMap.Right_Stick_Right);
            }
        }
        else
        {
            Console.WriteLine("Controller not connected. Pair wireless controllers manually to PC");
        }
    }
}
