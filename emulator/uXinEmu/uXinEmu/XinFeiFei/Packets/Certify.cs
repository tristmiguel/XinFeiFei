using System.Linq;

namespace uXinEmu.XinFeiFei
{
	// ReSharper disable ClassNeverInstantiated.Global

	internal partial class FFConnection
	{
		private void OnCertify(Packet packet)
		{
			var name = packet.ReadString();
			var password = packet.ReadString();

			//TODO: Find out what all the unused fields are and use them.

			var hdsn = packet.ReadString();
			var ipKey = packet.ReadBytes(21);
			var version = packet.ReadUInt32();
			var realVersion = packet.ReadUInt32(); 
			var opFlag = packet.ReadByte();
			var otpPassword = packet.ReadString();

			var user = Database.Users.FirstOrDefault(c => c.Name == name);

			//TODO: Add ECard.

			if (user != null)
			{
				if(user.Password == password)
				{
					if (!Server.Connections.Any(c => c.User != null && c.User.Name == user.Name))
					{
						if (realVersion == Constants.Version)
						{
							User = user;

							var result = new Packet(Opcode.CERTIFYRESULT);
							result.WriteInt32((int) Error.CERT_OK);
							result.WriteInt32(0); //Not required to fill in real name.
							result.WriteBoolean(false); //Expansion
							Send(result);

							CharacterList();
						}
						else
						{
							var result = new Packet(Opcode.CERTIFYRESULT);
							result.WriteInt32((int) (realVersion < Constants.Version ? Error.ERR_VERSION_TOO_LOW : Error.ERR_VERSION_MAINTAIN));
							Send(result);

							Disconnect();
						}
					}
					else
					{
						var result = new Packet(Opcode.CERTIFYRESULT);
						result.WriteInt32((int) Error.ERR_ACCOUNT_EXIST);
						Send(result);

						Disconnect();
					}
				}
				else
				{
					var result = new Packet(Opcode.CERTIFYRESULT);
					result.WriteInt32((int) ((uint) Error.CERT_CHARGE_CERTIFY_FAILED << 16));
					Send(result);
				}
			}
			else
			{
				var result = new Packet(Opcode.CERTIFYRESULT);
				result.WriteInt32((int) ((uint)Error.CERT_CHARGE_CERTIFY_FAILED << 16 | 1));
				Send(result);
			}
		}
	}
}
