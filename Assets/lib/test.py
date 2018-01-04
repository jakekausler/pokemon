import os
import struct

print os.listdir('.')

with open(u'Pok\xe9mon Essentials v17.2 2017-10-15/Data/moves.dat', 'rb') as f:
    moveid = 10
    pos = moveid*14
    data = f.read()
    print 'Function', hex(struct.unpack("BB", data[pos:pos+2])[0])
    print 'Base Damage', struct.unpack("B", data[pos+2:pos+3])[0]
    print 'Type', struct.unpack("B", data[pos+3:pos+4])[0]
    print 'Category', struct.unpack("B", data[pos+4:pos+5])[0]
    print 'Accuracy', struct.unpack("B", data[pos+5:pos+6])[0]
    print 'TotalPP', struct.unpack("B", data[pos+6:pos+7])[0]
    print 'Add1Effect', struct.unpack("B", data[pos+7:pos+8])[0]
    print 'Target', struct.unpack("BB", data[pos+8:pos+10])[0]
    print 'Priority', struct.unpack("b", data[pos+10:pos+11])[0]
    print 'Flags', struct.unpack("BB", data[pos+11:pos+13])[0]&0x00010
