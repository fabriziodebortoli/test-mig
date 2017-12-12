using System.Collections;

namespace Microarea.TaskBuilderNet.Licence.Activation.Components
{
	/// <summary>
	/// Calcola il CRC.
	/// </summary>
	//=========================================================================
	internal class Crc
	{
		private	static	bool	initialized	= false;

		// Checksum chars (use same char set of input strings).
		private static	char[]	checksums	= new char[]{
			'1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I',
			'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
			'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0'
			};

		// Caches to compute checksum.
		private	static Hashtable cache0;
		private	static Hashtable cache1;
		private	static Hashtable cache2;
		private	static Hashtable cache3;
		private	static Hashtable cache4;
		private	static Hashtable cache5;
		private	static Hashtable cache6;
		private	static Hashtable cache7;
		private	static Hashtable cache8;
		private	static Hashtable cache9;
		private	static Hashtable cache10;
		private	static Hashtable cache11;
		private	static Hashtable cache12;
		private	static Hashtable cache13;
		private	static Hashtable cache14;
		private	static Hashtable cache15;

		//---------------------------------------------------------------------
		private Crc()
		{}

		//---------------------------------------------------------------------
		private static void InitCaches()
		{
			cache0 = new Hashtable();
			cache0.Add('A', 0);cache0.Add('B', 1);cache0.Add('C', 2);cache0.Add('D', 3);cache0.Add('E', 4);
			cache0.Add('F', 5);cache0.Add('G', 6);cache0.Add('H', 7);cache0.Add('I', 8);cache0.Add('J', 14);
			cache0.Add('K', 10);cache0.Add('L', 11);cache0.Add('M', 12);cache0.Add('N', 13);cache0.Add('O', 9);
			cache0.Add('P', 15);cache0.Add('Q', 16);cache0.Add('R', 17);cache0.Add('S', 18);cache0.Add('T', 19);
			cache0.Add('U', 20);cache0.Add('V', 21);cache0.Add('W', 22);cache0.Add('X', 23);cache0.Add('Y', 24);
			cache0.Add('Z', 25);cache0.Add('0', 26);cache0.Add('1', 27);cache0.Add('2', 28);cache0.Add('3', 29);
			cache0.Add('4', 30);cache0.Add('5', 31);cache0.Add('6', 32);cache0.Add('7', 33);cache0.Add('8', 34);
			cache0.Add('9', 35);

			cache1 = new Hashtable();
			cache1.Add('A', 1);cache1.Add('B', 3);cache1.Add('C', 6);cache1.Add('D', 11);cache1.Add('E', 21);
			cache1.Add('F', 2);cache1.Add('G', 5);cache1.Add('H', 23);cache1.Add('I', 31);cache1.Add('J', 35);
			cache1.Add('K', 0);cache1.Add('L', 4);cache1.Add('M', 28);cache1.Add('N', 25);cache1.Add('O', 18);
			cache1.Add('P', 20);cache1.Add('Q', 14);cache1.Add('R', 33);cache1.Add('S', 34);cache1.Add('T', 32);
			cache1.Add('U', 12);cache1.Add('V', 13);cache1.Add('W', 30);cache1.Add('X', 17);cache1.Add('Y', 19);
			cache1.Add('Z', 15);cache1.Add('0', 16);cache1.Add('1', 9);cache1.Add('2', 8);cache1.Add('3', 10);
			cache1.Add('4', 22);cache1.Add('5', 27);cache1.Add('6', 29);cache1.Add('7', 7);cache1.Add('8', 24);
			cache1.Add('9', 26);

			cache2 = new Hashtable();
			cache2.Add('A', 23);cache2.Add('B', 28);cache2.Add('C', 13);cache2.Add('D', 19);cache2.Add('E', 31);
			cache2.Add('F', 22);cache2.Add('G', 4);cache2.Add('H', 11);cache2.Add('I', 21);cache2.Add('J', 16);
			cache2.Add('K', 7);cache2.Add('L', 0);cache2.Add('M', 5);cache2.Add('N', 18);cache2.Add('O', 33);
			cache2.Add('P', 17);cache2.Add('Q', 29);cache2.Add('R', 20);cache2.Add('S', 27);cache2.Add('T', 24);
			cache2.Add('U', 9);cache2.Add('V', 32);cache2.Add('W', 3);cache2.Add('X', 8);cache2.Add('Y', 35);
			cache2.Add('Z', 6);cache2.Add('0', 1);cache2.Add('1', 10);cache2.Add('2', 25);cache2.Add('3', 15);
			cache2.Add('4', 12);cache2.Add('5', 14);cache2.Add('6', 30);cache2.Add('7', 2);cache2.Add('8', 26);
			cache2.Add('9', 34);

			cache3 = new Hashtable();
			cache3.Add('A', 13);cache3.Add('B', 21);cache3.Add('C', 25);cache3.Add('D', 32);cache3.Add('E', 14);
			cache3.Add('F', 29);cache3.Add('G', 19);cache3.Add('H', 0);cache3.Add('I', 10);cache3.Add('J', 23);
			cache3.Add('K', 26);cache3.Add('L', 27);cache3.Add('M', 31);cache3.Add('N', 33);cache3.Add('O', 22);
			cache3.Add('P', 30);cache3.Add('Q', 1);cache3.Add('R', 2);cache3.Add('S', 11);cache3.Add('T', 4);
			cache3.Add('U', 35);cache3.Add('V', 5);cache3.Add('W', 16);cache3.Add('X', 34);cache3.Add('Y', 7);
			cache3.Add('Z', 8);cache3.Add('0', 3);cache3.Add('1', 17);cache3.Add('2', 9);cache3.Add('3', 28);
			cache3.Add('4', 6);cache3.Add('5', 12);cache3.Add('6', 15);cache3.Add('7', 18);cache3.Add('8', 20);
			cache3.Add('9', 24);

			cache4 = new Hashtable();
			cache4.Add('A', 35);cache4.Add('B', 16);cache4.Add('C', 27);cache4.Add('D', 14);cache4.Add('E', 30);
			cache4.Add('F', 12);cache4.Add('G', 34);cache4.Add('H', 18);cache4.Add('I', 6);cache4.Add('J', 28);
			cache4.Add('K', 20);cache4.Add('L', 21);cache4.Add('M', 22);cache4.Add('N', 0);cache4.Add('O', 2);
			cache4.Add('P', 4);cache4.Add('Q', 33);cache4.Add('R', 31);cache4.Add('S', 1);cache4.Add('T', 15);
			cache4.Add('U', 29);cache4.Add('V', 8);cache4.Add('W', 10);cache4.Add('X', 32);cache4.Add('Y', 26);
			cache4.Add('Z', 17);cache4.Add('0', 7);cache4.Add('1', 23);cache4.Add('2', 11);cache4.Add('3', 13);
			cache4.Add('4', 19);cache4.Add('5', 5);cache4.Add('6', 25);cache4.Add('7', 24);cache4.Add('8', 9);
			cache4.Add('9', 3);

			cache5 = new Hashtable();
			cache5.Add('A', 28);cache5.Add('B', 35);cache5.Add('C', 18);cache5.Add('D', 23);cache5.Add('E', 17);
			cache5.Add('F', 30);cache5.Add('G', 27);cache5.Add('H', 34);cache5.Add('I', 0);cache5.Add('J', 13);
			cache5.Add('K', 33);cache5.Add('L', 31);cache5.Add('M', 9);cache5.Add('N', 4);cache5.Add('O', 7);
			cache5.Add('P', 1);cache5.Add('Q', 11);cache5.Add('R', 29);cache5.Add('S', 26);cache5.Add('T', 8);
			cache5.Add('U', 16);cache5.Add('V', 19);cache5.Add('W', 20);cache5.Add('X', 12);cache5.Add('Y', 14);
			cache5.Add('Z', 32);cache5.Add('0', 21);cache5.Add('1', 24);cache5.Add('2', 15);cache5.Add('3', 2);
			cache5.Add('4', 10);cache5.Add('5', 25);cache5.Add('6', 6);cache5.Add('7', 5);cache5.Add('8', 3);
			cache5.Add('9', 22);

			cache6 = new Hashtable();
			cache6.Add('A', 16);cache6.Add('B', 0);cache6.Add('C', 21);cache6.Add('D', 9);cache6.Add('E', 26);
			cache6.Add('F', 33);cache6.Add('G', 8);cache6.Add('H', 35);cache6.Add('I', 22);cache6.Add('J', 7);
			cache6.Add('K', 13);cache6.Add('L', 1);cache6.Add('M', 17);cache6.Add('N', 34);cache6.Add('O', 20);
			cache6.Add('P', 23);cache6.Add('Q', 12);cache6.Add('R', 11);cache6.Add('S', 25);cache6.Add('T', 31);
			cache6.Add('U', 19);cache6.Add('V', 15);cache6.Add('W', 29);cache6.Add('X', 14);cache6.Add('Y', 32);
			cache6.Add('Z', 2);cache6.Add('0', 18);cache6.Add('1', 30);cache6.Add('2', 6);cache6.Add('3', 4);
			cache6.Add('4', 28);cache6.Add('5', 3);cache6.Add('6', 27);cache6.Add('7', 3);cache6.Add('8', 5);
			cache6.Add('9', 5);

			cache7 = new Hashtable();
			cache7.Add('A', 5);cache7.Add('B', 9);cache7.Add('C', 35);cache7.Add('D', 20);cache7.Add('E', 12);
			cache7.Add('F', 27);cache7.Add('G', 0);cache7.Add('H', 33);cache7.Add('I', 14);cache7.Add('J', 19);
			cache7.Add('K', 1);cache7.Add('L', 13);cache7.Add('M', 10);cache7.Add('N', 32);cache7.Add('O', 16);
			cache7.Add('P', 11);cache7.Add('Q', 17);cache7.Add('R', 25);cache7.Add('S', 31);cache7.Add('T', 29);
			cache7.Add('U', 21);cache7.Add('V', 34);cache7.Add('W', 28);cache7.Add('X', 30);cache7.Add('Y', 8);
			cache7.Add('Z', 24);cache7.Add('0', 23);cache7.Add('1', 2);cache7.Add('2', 7);cache7.Add('3', 3);
			cache7.Add('4', 26);cache7.Add('5', 6);cache7.Add('6', 22);cache7.Add('7', 15);cache7.Add('8', 18);
			cache7.Add('9', 4);

			cache8 = new Hashtable();
			cache8.Add('A', 2);cache8.Add('B', 7);cache8.Add('C', 0);cache8.Add('D', 1);cache8.Add('E', 19);
			cache8.Add('F', 32);cache8.Add('G', 16);cache8.Add('H', 9);cache8.Add('I', 3);cache8.Add('J', 11);
			cache8.Add('K', 25);cache8.Add('L', 17);cache8.Add('M', 29);cache8.Add('N', 35);cache8.Add('O', 24);
			cache8.Add('P', 13);cache8.Add('Q', 15);cache8.Add('R', 23);cache8.Add('S', 4);cache8.Add('T', 22);
			cache8.Add('U', 26);cache8.Add('V', 31);cache8.Add('W', 34);cache8.Add('X', 28);cache8.Add('Y', 33);
			cache8.Add('Z', 5);cache8.Add('0', 8);cache8.Add('1', 21);cache8.Add('2', 30);cache8.Add('3', 27);
			cache8.Add('4', 20);cache8.Add('5', 10);cache8.Add('6', 14);cache8.Add('7', 6);cache8.Add('8', 12);
			cache8.Add('9', 18);

			cache9 = new Hashtable();
			cache9.Add('A', 22);cache9.Add('B', 14);cache9.Add('C', 29);cache9.Add('D', 6);cache9.Add('E', 9);
			cache9.Add('F', 16);cache9.Add('G', 11);cache9.Add('H', 13);cache9.Add('I', 7);cache9.Add('J', 27);
			cache9.Add('K', 15);cache9.Add('L', 18);cache9.Add('M', 30);cache9.Add('N', 1);cache9.Add('O', 0);
			cache9.Add('P', 21);cache9.Add('Q', 24);cache9.Add('R', 34);cache9.Add('S', 2);cache9.Add('T', 5);
			cache9.Add('U', 4);cache9.Add('V', 23);cache9.Add('W', 35);cache9.Add('X', 31);cache9.Add('Y', 28);
			cache9.Add('Z', 19);cache9.Add('0', 33);cache9.Add('1', 26);cache9.Add('2', 12);cache9.Add('3', 20);
			cache9.Add('4', 3);cache9.Add('5', 17);cache9.Add('6', 8);cache9.Add('7', 25);cache9.Add('8', 32);
			cache9.Add('9', 10);

			cache10 = new Hashtable();
			cache10.Add('A', 3);cache10.Add('B', 2);cache10.Add('C', 1);cache10.Add('D', 0);cache10.Add('E', 22);
			cache10.Add('F', 35);cache10.Add('G', 12);cache10.Add('H', 26);cache10.Add('I', 19);cache10.Add('J', 34);
			cache10.Add('K', 11);cache10.Add('L', 20);cache10.Add('M', 27);cache10.Add('N', 5);cache10.Add('O', 21);
			cache10.Add('P', 14);cache10.Add('Q', 13);cache10.Add('R', 28);cache10.Add('S', 29);cache10.Add('T', 33);
			cache10.Add('U', 32);cache10.Add('V', 25);cache10.Add('W', 9);cache10.Add('X', 18);cache10.Add('Y', 23);
			cache10.Add('Z', 31);cache10.Add('0', 15);cache10.Add('1', 8);cache10.Add('2', 10);cache10.Add('3', 17);
			cache10.Add('4', 7);cache10.Add('5', 16);cache10.Add('6', 24);cache10.Add('7', 4);cache10.Add('8', 30);
			cache10.Add('9', 6);

			cache11 = new Hashtable();
			cache11.Add('A', 8);cache11.Add('B', 34);cache11.Add('C', 16);cache11.Add('D', 15);cache11.Add('E', 2);
			cache11.Add('F', 10);cache11.Add('G', 18);cache11.Add('H', 29);cache11.Add('I', 24);cache11.Add('J', 6);
			cache11.Add('K', 3);cache11.Add('L', 26);cache11.Add('M', 19);cache11.Add('N', 7);cache11.Add('O', 1);
			cache11.Add('P', 33);cache11.Add('Q', 35);cache11.Add('R', 12);cache11.Add('S', 0);cache11.Add('T', 21);
			cache11.Add('U', 13);cache11.Add('V', 22);cache11.Add('W', 27);cache11.Add('X', 25);cache11.Add('Y', 11);
			cache11.Add('Z', 4);cache11.Add('0', 5);cache11.Add('1', 20);cache11.Add('2', 23);cache11.Add('3', 31);
			cache11.Add('4', 32);cache11.Add('5', 9);cache11.Add('6', 17);cache11.Add('7', 28);cache11.Add('8', 14);
			cache11.Add('9', 3);

			cache12 = new Hashtable();
			cache12.Add('A', 17);cache12.Add('B', 23);cache12.Add('C', 9);cache12.Add('D', 21);cache12.Add('E', 35);
			cache12.Add('F', 0);cache12.Add('G', 3);cache12.Add('H', 4);cache12.Add('I', 11);cache12.Add('J', 12);
			cache12.Add('K', 31);cache12.Add('L', 19);cache12.Add('M', 16);cache12.Add('N', 28);cache12.Add('O', 34);
			cache12.Add('P', 18);cache12.Add('Q', 10);cache12.Add('R', 8);cache12.Add('S', 5);cache12.Add('T', 1);
			cache12.Add('U', 14);cache12.Add('V', 26);cache12.Add('W', 32);cache12.Add('X', 15);cache12.Add('Y', 29);
			cache12.Add('Z', 33);cache12.Add('0', 22);cache12.Add('1', 7);cache12.Add('2', 24);cache12.Add('3', 25);
			cache12.Add('4', 2);cache12.Add('5', 13);cache12.Add('6', 20);cache12.Add('7', 30);cache12.Add('8', 6);
			cache12.Add('9', 27);

			cache13 = new Hashtable();
			cache13.Add('A', 19);cache13.Add('B', 27);cache13.Add('C', 34);cache13.Add('D', 2);cache13.Add('E', 3);
			cache13.Add('F', 11);cache13.Add('G', 15);cache13.Add('H', 12);cache13.Add('I', 26);cache13.Add('J', 21);
			cache13.Add('K', 35);cache13.Add('L', 29);cache13.Add('M', 24);cache13.Add('N', 17);cache13.Add('O', 10);
			cache13.Add('P', 16);cache13.Add('Q', 20);cache13.Add('R', 30);cache13.Add('S', 28);cache13.Add('T', 9);
			cache13.Add('U', 22);cache13.Add('V', 1);cache13.Add('W', 25);cache13.Add('X', 13);cache13.Add('Y', 31);
			cache13.Add('Z', 23);cache13.Add('0', 6);cache13.Add('1', 4);cache13.Add('2', 7);cache13.Add('3', 33);
			cache13.Add('4', 14);cache13.Add('5', 18);cache13.Add('6', 3);cache13.Add('7', 32);cache13.Add('8', 8);
			cache13.Add('9', 0);

			cache14 = new Hashtable();
			cache14.Add('A', 34);cache14.Add('B', 19);cache14.Add('C', 8);cache14.Add('D', 16);cache14.Add('E', 20);
			cache14.Add('F', 13);cache14.Add('G', 17);cache14.Add('H', 21);cache14.Add('I', 18);cache14.Add('J', 22);
			cache14.Add('K', 14);cache14.Add('L', 12);cache14.Add('M', 0);cache14.Add('N', 15);cache14.Add('O', 29);
			cache14.Add('P', 10);cache14.Add('Q', 9);cache14.Add('R', 32);cache14.Add('S', 35);cache14.Add('T', 26);
			cache14.Add('U', 1);cache14.Add('V', 7);cache14.Add('W', 11);cache14.Add('X', 3);cache14.Add('Y', 25);
			cache14.Add('Z', 28);cache14.Add('0', 31);cache14.Add('1', 5);cache14.Add('2', 2);cache14.Add('3', 23);
			cache14.Add('4', 4);cache14.Add('5', 24);cache14.Add('6', 33);cache14.Add('7', 9);cache14.Add('8', 27);
			cache14.Add('9', 30);

			cache15 = new Hashtable();
			cache15.Add('A', 4);cache15.Add('B', 26);cache15.Add('C', 15);cache15.Add('D', 18);cache15.Add('E', 10);
			cache15.Add('F', 17);cache15.Add('G', 9);cache15.Add('H', 14);cache15.Add('I', 20);cache15.Add('J', 5);
			cache15.Add('K', 2);cache15.Add('L', 3);cache15.Add('M', 11);cache15.Add('N', 6);cache15.Add('O', 12);
			cache15.Add('P', 0);cache15.Add('Q', 7);cache15.Add('R', 1);cache15.Add('S', 8);cache15.Add('T', 13);
			cache15.Add('U', 23);cache15.Add('V', 16);cache15.Add('W', 19);cache15.Add('X', 21);cache15.Add('Y', 26);
			cache15.Add('Z', 27);cache15.Add('0', 24);cache15.Add('1', 25);cache15.Add('2', 29);cache15.Add('3', 30);
			cache15.Add('4', 31);cache15.Add('5', 28);cache15.Add('6', 34);cache15.Add('7', 35);cache15.Add('8', 33);
			cache15.Add('9', 32);
		}

		//---------------------------------------------------------------------
		public static char ComputeCrc(string toBeChecked)
		{
			if (!initialized)
			{
				lock (typeof(Crc))
				{
					InitCaches();
					initialized = true;
				}
			}

			int checksumValue = 0;
			for (int i = 0; i < toBeChecked.Length; i++)
			{
				switch (i % 16)
				{
					case 0:checksumValue += (int)cache0[toBeChecked[i]];break;
					case 1:checksumValue += (int)cache1[toBeChecked[i]];break;
					case 2:checksumValue += (int)cache2[toBeChecked[i]];break;
					case 3:checksumValue += (int)cache3[toBeChecked[i]];break;
					case 4:checksumValue += (int)cache4[toBeChecked[i]];break;
					case 5:checksumValue += (int)cache5[toBeChecked[i]];break;
					case 6:checksumValue += (int)cache6[toBeChecked[i]];break;
					case 7:checksumValue += (int)cache7[toBeChecked[i]];break;
					case 8:checksumValue += (int)cache8[toBeChecked[i]];break;
					case 9:checksumValue += (int)cache9[toBeChecked[i]];break;
					case 10:checksumValue += (int)cache10[toBeChecked[i]];break;
					case 11:checksumValue += (int)cache11[toBeChecked[i]];break;
					case 12:checksumValue += (int)cache12[toBeChecked[i]];break;
					case 13:checksumValue += (int)cache13[toBeChecked[i]];break;
					case 14:checksumValue += (int)cache14[toBeChecked[i]];break;
					case 15:checksumValue += (int)cache15[toBeChecked[i]];break;
				}
			}

			checksumValue = checksumValue % checksums.Length;
			return checksums[checksumValue];
		}
	}
}
