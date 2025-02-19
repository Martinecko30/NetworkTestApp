using System.Text;

namespace NetworkTestApp.Shared;

public class Packet
{
    private string senderAlias = "default";
    private MessageType type = MessageType.None;
    private string message = string.Empty;
    
    const int ALIAS_LENGTH = 30;
    const int TYPE_LENGTH = 1;
    const int MESSAGE_LENGTH = 4096;

    public Packet(string senderAlias, MessageType type, string message)
    {
        this.senderAlias = senderAlias;
        this.type = type;
        this.message = message;
    }

    public Packet(byte[] rawBytes)
    {
        // if (rawBytes.Length < ALIAS_LENGTH + TYPE_LENGTH + MESSAGE_LENGTH)
        // {
        //     Console.WriteLine("Packet contained invalid rawBytes length.\nIgnoring packet.");
        //     return;
        // }
        
        try
        {
            byte[] bytesSenderAlias = rawBytes.Take(ALIAS_LENGTH).ToArray();
            this.senderAlias = Encoding.ASCII.GetString(bytesSenderAlias).TrimEnd('\0'); // Remove null padding

            this.type = (MessageType)rawBytes[ALIAS_LENGTH]; 

            byte[] bytesMessage = rawBytes.Skip(ALIAS_LENGTH + TYPE_LENGTH).Take(MESSAGE_LENGTH).ToArray();
            this.message = Encoding.ASCII.GetString(bytesMessage).TrimEnd('\0'); // Remove null padding
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("An error occured.");
            return;
        }
    }

    public byte[] ToByteArray()
    {
        byte[] bytesSenderAlias = new byte[ALIAS_LENGTH];
        byte[] bytesType = new byte[TYPE_LENGTH];
        byte[] bytesMessage = new byte[MESSAGE_LENGTH];
        
        try
        {
            byte[] tempAlias = Encoding.ASCII.GetBytes(senderAlias);
            Array.Copy(tempAlias, bytesSenderAlias, Math.Min(tempAlias.Length, ALIAS_LENGTH));

            bytesType[0] = (byte)type;

            byte[] tempMessage = Encoding.ASCII.GetBytes(message);
            Array.Copy(tempMessage, bytesMessage, Math.Min(tempMessage.Length, MESSAGE_LENGTH));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("An error occured.");
            return new byte[0];
        }
        
        return bytesSenderAlias.Concat(bytesType).Concat(bytesMessage).ToArray();
    }

    public override string ToString()
    {
        return $"({type}) {senderAlias}: {message}";
    }
    
    public static int Length => ALIAS_LENGTH + TYPE_LENGTH + MESSAGE_LENGTH;
}