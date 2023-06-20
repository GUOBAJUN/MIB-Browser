using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MIB_Browser;

public class MIB_Browser
{
    private string IP
    {
        get; set;
    }
    private string OID
    {
        get; set;
    }
    private string Community
    {
        get; set;
    }
    private int Timeout
    {
        get; set;
    }

    private int MaxRepetitions
    {
        get; set;
    }

    private int request_id = 0;

    public ObservableCollection<string> OID_History
    {
        get; set;
    }
    public MIB_Browser(string ip = "127.0.0.1", string oid = "1.3.6.1.2.1.1.1.0", string community = "public", int timeout = 2000, int maxRepetitions = 10)
    {
        IP = ip;
        OID = oid;
        Community = community;
        Timeout = timeout;
        OID_History = new ObservableCollection<string>();
        OID_History.Insert(0, oid);
        MaxRepetitions = maxRepetitions;
    }

    public IList<Variable> GetRequest()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(IP), 161);
        List<Variable> variables = new List<Variable>
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
        catch (Exception)
        {
            throw;
        }
        return null;
    }

    public IList<Variable> GetNextRequest()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(IP), 161);
        List<Variable> variables = new List<Variable>
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
        catch (Exception)
        {
            throw;
        }
        return null;
    }

    public IList<Variable> GetBulk()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(IP), 161);
        List<Variable> variables = new List<Variable>
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

    public void SetIP(string ip) => IP = ip;
    public void SetOID(string oid) => OID = oid;
    public void SetCommunity(string community) => Community = community;
    public void SetTimeout(int timeout) => Timeout = timeout;
    public void SetMaxRepetitions(int maxRepetitions) => MaxRepetitions = maxRepetitions;
    public string GetOID() => OID;
    public override string ToString()
    {
        return IP + ":" + OID + ":" + Community + ":" + Timeout;
    }
}