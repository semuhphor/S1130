using Xunit;
using S1130.SystemObjects;

namespace UnitTests.S1130.SystemObjects
{
    public class AssemblerConverterTests
    {
        [Fact]
        public void ToIBM_SimpleLoadInstruction_ShortFormat()
        {
            var s1130 = "      LD   . VALUE";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Columns: 1-20(blank), 21-25(label="     "), 26(blank), 27-30(op="LD  "), 31(blank), 32(format=" "), 33(tag=" "), 34(blank), 35+(operand="VALUE")
            Assert.Equal("                          LD      VALUE", ibm);
        }

        [Fact]
        public void ToIBM_LoadInstruction_LongFormat()
        {
            var s1130 = "      LD   L VALUE";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Col 32='L', col 33=' '
            Assert.Equal("                          LD   L  VALUE", ibm);
        }

        [Fact]
        public void ToIBM_LoadInstruction_WithLabel()
        {
            var s1130 = "START LD   L VALUE";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Col 21-25="START", col 27-30="LD  ", col 32="L"
            Assert.Equal("                    START LD   L  VALUE", ibm);
        }

        [Fact]
        public void ToIBM_LoadInstruction_WithIndexRegister()
        {
            var s1130 = "      LD   1 TABLE";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Short format with XR1: col 32=' ', col 33='1'
            Assert.Equal("                          LD    1 TABLE", ibm);
        }

        [Fact]
        public void ToIBM_LoadInstruction_LongWithIndex()
        {
            var s1130 = "      LD   L2 TABLE";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Long format with XR2: col 32='L', col 33='2'
            Assert.Equal("                          LD   L2 TABLE", ibm);
        }

        [Fact]
        public void ToIBM_LoadInstruction_Indirect()
        {
            var s1130 = "      LD   I PTR";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Indirect: col 32='I'
            Assert.Equal("                          LD   I  PTR", ibm);
        }

        [Fact]
        public void ToIBM_LoadInstruction_IndirectWithIndex()
        {
            var s1130 = "      LD   I1 PTR";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Indirect with XR1: col 32='I', col 33='1'
            Assert.Equal("                          LD   I1 PTR", ibm);
        }

        [Fact]
        public void ToIBM_CommentLine()
        {
            var s1130 = "* This is a comment";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            Assert.Equal("                    * This is a comment", ibm);
        }

        [Fact]
        public void ToIBM_InlineComment()
        {
            var s1130 = "      LD   L VALUE    // Load the value";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Inline comment should be stripped
            Assert.Equal("                          LD   L  VALUE", ibm);
        }

        [Fact]
        public void ToIBM_Directive_ORG()
        {
            var s1130 = "      ORG  /100";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Col 27-30="ORG ", no format/tag for directive
            Assert.Equal("                          ORG     /100", ibm);
        }

        [Fact]
        public void ToIBM_Directive_DC()
        {
            var s1130 = "VALUE DC   42";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            // Col 21-25="VALUE", col 27-30="DC  "
            Assert.Equal("                    VALUE DC      42", ibm);
        }

        [Fact]
        public void ToIBM_BSC_ShortFormat()
        {
            var s1130 = "      BSC  . O";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            Assert.Equal("                          BSC     O", ibm);
        }

        [Fact]
        public void ToIBM_BSC_LongFormat()
        {
            var s1130 = "      BSC  L O LOOP";
            var ibm = AssemblerConverter.ToIBM1130Format(s1130).TrimEnd();
            
            Assert.Equal("                          BSC  L  O LOOP", ibm);
        }

        [Fact]
        public void ToS1130_SimpleLoadInstruction()
        {
            // Col 27-30="LD  ", col 32-33=blank (short format), col 35+="VALUE"
            var ibm = "                          LD      VALUE";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      LD  . VALUE", s1130);
        }

        [Fact]
        public void ToS1130_LoadInstruction_LongFormat()
        {
            // Col 27-30="LD  ", col 32="L", col 35+="VALUE"
            var ibm = "                          LD   L  VALUE";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      LD  L VALUE", s1130);
        }

        [Fact]
        public void ToS1130_LoadInstruction_WithLabel()
        {
            // Col 21-25="START", col 27-30="LD  ", col 32="L"
            var ibm = "                    START LD   L  VALUE";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("START  LD  L VALUE", s1130);
        }

        [Fact]
        public void ToS1130_LoadInstruction_WithIndexRegister()
        {
            // Col 32=blank, col 33='1'
            var ibm = "                          LD    1 TABLE";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      LD  1 TABLE", s1130);
        }

        [Fact]
        public void ToS1130_LoadInstruction_LongWithIndex()
        {
            // Col 32='L', col 33='2'
            var ibm = "                          LD   L2 TABLE";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      LD  L2 TABLE", s1130);
        }

        [Fact]
        public void ToS1130_LoadInstruction_Indirect()
        {
            // Col 32='I'
            var ibm = "                          LD   I  PTR";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      LD  I PTR", s1130);
        }

        [Fact]
        public void ToS1130_LoadInstruction_IndirectWithIndex()
        {
            // Col 32='I', col 33='1'
            var ibm = "                          LD   I1 PTR";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      LD  I1 PTR", s1130);
        }

        [Fact]
        public void ToS1130_CommentLine()
        {
            var ibm = "                    * This is a comment";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("* This is a comment", s1130);
        }

        [Fact]
        public void ToS1130_Directive_ORG()
        {
            var ibm = "                          ORG     /100";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("      ORG /100", s1130);
        }

        [Fact]
        public void ToS1130_Directive_DC()
        {
            var ibm = "                    VALUE DC      42";
            var s1130 = AssemblerConverter.ToS1130Format(ibm).TrimEnd();
            
            Assert.Equal("VALUE  DC 42", s1130);
        }

        [Fact]
        public void RoundTrip_CompleteProgram()
        {
            var original = @"      ORG  /100
START LD   L VALUE
      A    L ONE
      STO  L RESULT
      WAIT
VALUE DC   5
ONE   DC   1
RESULT DC  0";

            var ibmFormat = AssemblerConverter.ToIBM1130Format(original);
            var backToS1130 = AssemblerConverter.ToS1130Format(ibmFormat);
            
            // Should be functionally equivalent (may have formatting differences)
            Assert.Contains("ORG", backToS1130);
            Assert.Contains("START", backToS1130);
            Assert.Contains("VALUE", backToS1130);
            Assert.Contains("WAIT", backToS1130);
        }
    }
}
