using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MIB_Browser;

public class MibBrowser
{
    public string AgentIP
    {
        get; set;
    }
    public string OID
    {
        get; set;
    }
    public string Community
    {
        get; set;
    }
    public int Timeout
    {
        get; set;
    }
    public int MaxRepetitions
    {
        get; set;
    }
    public int request_id = 0;

    public MibBrowser(string agentIP = "127.0.0.1", string oid = "1.3.6.1.2.1.1.1.0", string community = "public", int timeout = 2000, int maxRepetitions = 10)
    {
        AgentIP = agentIP;
        OID = oid;
        Community = community;
        Timeout = timeout;
        MaxRepetitions = maxRepetitions;
    }

    public IList<Variable> GetRequest()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(AgentIP), 161);
        IList<Variable> variables = new List<Variable>
        {
            new Variable(new ObjectIdentifier(OID))
        };
        GetRequestMessage request = new GetRequestMessage(request_id++, VersionCode.V1, new OctetString(Community), variables);
        ISnmpMessage response;
        try
        {
            response = request.GetResponse(Timeout, receiver);
            if (response != null)
            {
                return response.Pdu().Variables;
            }
        }
        catch (Exception) { throw; }
        return null;
    }

    public async Task<IList<Variable>> GetRequestAsync()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(AgentIP), 161);
        IList<Variable> variables = new List<Variable>
        {
            new Variable(new ObjectIdentifier(OID))
        };
        GetRequestMessage request = new GetRequestMessage(request_id++, VersionCode.V1, new OctetString(Community), variables);
        ISnmpMessage response;
        try
        {
            response = await request.GetResponseAsync(receiver, new System.Threading.CancellationTokenSource(Timeout).Token);
            if (response != null)
            {
                return response.Pdu().Variables;
            }
        }
        catch (Exception) { throw; }
        return null;
    }

    public IList<Variable> GetNextRequest()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(AgentIP), 161);
        IList<Variable> variables = new List<Variable>
        {
            new Variable(new ObjectIdentifier(OID))
        };
        GetNextRequestMessage request = new GetNextRequestMessage(request_id++, VersionCode.V1, new OctetString(Community), variables);
        ISnmpMessage response;
        try
        {
            response = request.GetResponse(Timeout, receiver);
            if (response != null)
            {
                return response.Pdu().Variables;
            }
        }
        catch (Exception) { throw; }
        return null;
    }

    public async Task<IList<Variable>> GetNextRequestAsync()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(AgentIP), 161);
        IList<Variable> variables = new List<Variable>
        {
            new Variable(new ObjectIdentifier(OID))
        };
        GetNextRequestMessage request = new GetNextRequestMessage(request_id++, VersionCode.V1, new OctetString(Community), variables);
        ISnmpMessage response;
        try
        {
            response = await request.GetResponseAsync(receiver, new System.Threading.CancellationTokenSource(Timeout).Token);
            if (response != null)
            {
                return response.Pdu().Variables;
            }
        }
        catch (Exception) { throw; }
        return null;
    }

    public List<Variable> GetTree()
    {
        List<Variable> variables = new List<Variable>();
        var Father = OID;
        try
        {
            do
            {
                var result = GetBulk();
                if (result != null)
                {
                    foreach (var variable in result)
                    {
                        if (variable.Id.ToString().StartsWith(Father))
                            variables.Add(variable);
                        else
                            return variables;
                    }
                }
                OID = result.Last().Id.ToString();
            } while (true);
        }
        catch (Exception) { throw; }
    }

    public async Task<List<Variable>> GetTreeAsync()
    {
        List<Variable> variables = new List<Variable>();
        var Father = OID;
        try
        {
            do
            {
                var result = await GetBulkAsync();
                if (result != null)
                {
                    foreach (var variable in result)
                    {
                        if (variable.Id.ToString().StartsWith(Father))
                            variables.Add(variable);
                        else
                            return variables;
                    }
                }
                OID = result.Last().Id.ToString();
            } while (true);
        }
        catch (Exception) { throw; }
    }

    public IList<Variable> GetBulk()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(AgentIP), 161);
        IList<Variable> variables = new List<Variable>
        {
            new Variable (new ObjectIdentifier(OID))
        };
        GetBulkRequestMessage request = new GetBulkRequestMessage(request_id++, VersionCode.V2, new OctetString(Community), 0, MaxRepetitions, variables);
        ISnmpMessage response;
        try
        {
            response = request.GetResponse(Timeout, receiver);
            if (response != null)
            {
                return response.Pdu().Variables;
            }
        }
        catch (Exception) { throw; }
        return null;
    }

    public async Task<IList<Variable>> GetBulkAsync()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(AgentIP), 161);
        IList<Variable> variables = new List<Variable>
        {
            new Variable (new ObjectIdentifier(OID))
        };
        GetBulkRequestMessage request = new GetBulkRequestMessage(request_id++, VersionCode.V2, new OctetString(Community), 0, MaxRepetitions, variables);
        ISnmpMessage response;
        try
        {
            response = await request.GetResponseAsync(receiver, new System.Threading.CancellationTokenSource(Timeout).Token);
            if (response != null)
            {
                return response.Pdu().Variables;
            }
        }
        catch (Exception) { throw; }
        return null;
    }
    public static uint IPtoUINT(string addr)
    {
        if (!IPAddress.TryParse(addr, out var ip)) return 0;
        var bInt = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bInt);
        return BitConverter.ToUInt32(bInt, 0);
    }

    public static string UINTtoIP(uint addr)
    {
        var addr1 = (byte)((addr & 0xFF000000) >> 0x18);
        var addr2 = (byte)((addr & 0x00FF0000) >> 0x10);
        var addr3 = (byte)((addr & 0x0000FF00) >> 0x08);
        var addr4 = (byte)((addr & 0x000000FF));
        return string.Format("{0}.{1}.{2}.{3}", addr1, addr2, addr3, addr4);
    }

    public override string ToString()
    {
        return AgentIP + ":" + OID + ":" + Community + ":" + Timeout;
    }
}