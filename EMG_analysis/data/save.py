# read from serial port and save to fil
import time
import sys
from serial import Serial

# open serial port
ser = Serial('COM5', 115200)

# Toggle DTR to reset Arduino
ser.setDTR(False)
time.sleep(1)
# Toggling DTR "resets" the Arduino
ser.flushInput()
ser.setDTR(True)

# open file
f = open('data.txt', 'w')
print('Saving data to data.txt')

# read from serial port
while True:
    try:
        data = ser.readline().decode('utf-8')
        nums = str(data).strip()
        print(nums)
        f.write(nums+'\n')
    except KeyboardInterrupt:
        f.close()
        ser.close()
        sys.exit()
