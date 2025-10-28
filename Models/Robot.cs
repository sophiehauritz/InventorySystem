using System.Net.Sockets;
using System.Text;

namespace InventorySystem.Models
{
    /// <summary>
    /// Minimal robot client for URSim. Sends a known-good URScript program
    /// that uses the OnRobot gripper via XML-RPC and runs a full pick/place
    /// sequence. Call Robot.RunSequence() when you want it to execute.
    /// </summary>
    public class Robot
    {
        // Host and ports (Docker typically maps these to localhost)
        public string IpAddress { get; set; } = "127.0.0.1"; // use "172.17.0.2" if you connect directly into the container
        public int UrsPort { get; set; } = 30002;            // secondary interface (URScript)
        public int DashboardPort { get; set; } = 29999;      // dashboard server

        /// <summary>
        /// Runs the predefined pick/place sequence (same motion you tested).
        /// </summary>
        public void RunSequence()
        {
            // IMPORTANT: The URL below must be plain ASCII (no hidden chars).
            // If your gripper bridge runs elsewhere, change the URL/port.
            const string program = @"
def f():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  # OnRobot RPC object. With RPC we can call functions inside the gripper.

  global TOOL_INDEX = 0
  # We have only one gripper.

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  # width: [0..110], force: [0..40]
  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + 0.0, force + 0.0)
    sleep(0.01)  # let it start before we poll
    while (rg_is_busy()):
    end
  end

  # ---- Sequence poses (meters + rotation vector) ----
  p1  = p[-0.131,  -0.295,  0.473,  2.5,   0.0,  0.030]
  p2  = p[ 0.01989,-0.4783,-0.130,  3.14,  0.0,  0.0]
  p3  = p[ 0.01989,-0.4783,-0.010,  3.14,  0.0,  0.0]
  p4  = p[-0.1248, -0.2864,-0.010,  3.14,  0.0,  0.0]
  p5  = p[-0.1248, -0.2864,-0.095,  3.14,  0.0,  0.0]
  p6  = p[-0.1248, -0.2864,-0.010,  3.14,  0.0,  0.0]
  p7  = p[-0.12427,-0.4836,-0.010,  3.14,  0.0,  0.0]
  p8  = p[-0.12427,-0.4836,-0.130,  3.14,  0.0,  0.0]
  p9  = p[-0.12427,-0.4836,-0.010,  3.14,  0.0,  0.0]
  p10 = p[-0.326,  -0.473, -0.010,  3.14,  0.0,  0.0]
  p11 = p[-0.326,  -0.473, -0.130,  3.14,  0.0,  0.0]

  times = 0
  while (times < 1):
    movej(get_inverse_kin(p1))

    movej(get_inverse_kin(p2))

    movel(get_inverse_kin(p3))
    movel(get_inverse_kin(p4))
    movel(get_inverse_kin(p5))

    movel(get_inverse_kin(p6))
    movel(get_inverse_kin(p7))

    movej(get_inverse_kin(p8))
    movel(get_inverse_kin(p9))
    movel(get_inverse_kin(p4))
    movel(get_inverse_kin(p5))

    movel(get_inverse_kin(p4))
    movel(get_inverse_kin(p10))
    movej(get_inverse_kin(p11))

    movel(get_inverse_kin(p10))
    movel(get_inverse_kin(p4))
    movel(get_inverse_kin(p5))

    movel(get_inverse_kin(p4))
    times = times + 1
  end
end

f()
";

            // Release brakes + send the program
            SendString(DashboardPort, "brake release\n");
            SendString(UrsPort, EnsureNl(program));
        }

        // ---------- Low-level helpers ----------

        private void SendString(int port, string message)
        {
            using var client = new TcpClient(IpAddress, port);
            using var stream = client.GetStream();
            var data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private static string EnsureNl(string s) => s.EndsWith("\n") ? s : s + "\n";

        // Optional convenience (if you ever want these)
        public void StopProgram()       => SendString(DashboardPort, "stop\n");
        public void Popup(string text)  => SendString(UrsPort, $"popup(\"{Escape(text)}\")\n");
        public void TextMsg(string txt) => SendString(UrsPort, $"textmsg(\"{Escape(txt)}\")\n");
        private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
