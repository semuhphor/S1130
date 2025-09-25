namespace S1130.SystemObjects
{
    public interface IDebugSetting
    {
        bool IsBreakpoint { get; set; }
        bool IsBreakpointConditional { get; set; }
        string BreakpointCondition { get; set; }
        string BreakpointComment { get; set; }
        bool IsTracepoint { get; set; }
        bool IsTracepointConditional { get; set; }
        string TracepointCondition { get; set; }
        string TracepointComment { get; set; }
    }
}