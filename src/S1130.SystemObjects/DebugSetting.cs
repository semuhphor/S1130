namespace S1130.SystemObjects
{
    public class DebugSetting : IDebugSetting
    {
        /*
			About DebugSetting.

			A debug setting is may be placed in the CPU's private DebugSettings array. 
			Each memory location has a corresponding DebugSetting location. If the master debug bool is true, then the CPU will check if a DebugSetting exists for the current memory location. 
			If it does, then the CPU will check if the breakpoint condition is met. If it is, then the CPU will issue a breakpoint event. 
			If the tracepoint condition is met, then the CPU issue a trace event.
			Note that both can be set so both a trace and breakpoint event will be issued.

			In both cases, the breakpoint or the tracepoint can be conditional. If the conditional bool is set, the CPU will evaluate the requested condition and issue the corresponding event only if the condition is true. 

			The conditon parser take takes statements such as:
			- "Acc == 0"
			- "Acc > 0"
			- "Acc <=  22"
			- "Carry == 1 && Acc == 0"
			- Direct memory testing. Examples include:
				- "mem[loc] == 0"
				- "mem[loc] <= 22"
				- "mem[loc] == 1 && Acc == 0"
			- Index register testing where Xr is a decimal (0) or hex (0h/0x0) value. Examples include:
				- "xr1 == 0"
				- "xr2 <= 22"
				- "xr3 == 1 && Acc == 0"			
			- Indirect memory testing . Examples include:
				- "mem[mem[loc]] == 0"
				- "mem[mem[xr3]] <= 22"
				- "mem[mem[Acc]] == 1 && Acc == 77"
			- Offsets are supported. Examples include:
				- "mem[loc+1] == 0"
				- "mem[loc-1] <= 22"
				- "mem[xr3]+10 == 1 && Acc == 0"
			- Both sides of the comparison are evaluated. Examples include:
				- "mem[loc] == mem[loc+1]"
				- "mem[loc] <= Acc"
				- "mem[xr3] == mem[xr3]+10 && Acc == 0x500"
			Locations and values can be expressed as decimal (256) or hex (0x100/100h)

			Note that contitional breakpoints and tracepoints will slow down executiong substantially. But that's debugging for ya.

			I don't know what the results of doing this in interrupt routines will be yet. It will probably work since this is not a real machine, but something that has a timeout (like a 1442 card read/punch) may get irritated.
			we'll see.

			In the initial version, you will probably need spaces around tokens and operators. I'll fix that later.
			Also, the initial version wil be pure interpolation of the conditionals. It will likely never compile, but it may get a byte code later for faster operation. 
			Let's get it working first.
		*/

        public bool IsBreakpoint { get; set; }
        public bool IsBreakpointConditional { get; set; }
        public string BreakpointCondition { get; set; }
        public string BreakpointComment { get; set; }
        public bool IsTracepoint { get; set; }
        public bool IsTracepointConditional { get; set; }
        public string TracepointCondition { get; set; }
        public string TracepointComment { get; set; }
    }
}