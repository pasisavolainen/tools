#!/usr/bin/env python3

from optparse import OptionParser
import subprocess

import datetime
from time import sleep

class ContainerInfo:
    def __init__(self, sname):
        names = sname.split(',')
        self.name = names[0]
        if len(names) > 1:
            self.shortname = names[1]
        else:
            self.shortname = self.name
        self.seentime = datetime.datetime(2000, 1, 1)
        self.newlines = []

    def update(self):
        #print ("*** Updating {s.name}:{s.shortname}".format(s=self))
        proc = subprocess.run(["docker", "logs", "-t", "--tail", "10", self.name],
            stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=False)

        # sqlserver linux image uses "\r\n" as newlines and that completely messes up the 
        # `docker logs -t` output (when output to console, it hides timestamps)
        sout = proc.stdout.decode("utf-8").replace("\r", "")
        serr = proc.stderr.decode("utf-8").replace("\r","")
        combinedoutput = sout + "\n" + serr

        if len(combinedoutput) < 5: # ie. not even timestamp
            self.newlines = []
            #print ("no out: " + self.shortname)
            return

        lines = combinedoutput.split("\n")

        templines = sorted([self.formatline(l) for l in lines if l != ""])
        # there is always at least one line
        oldesttime = templines[-1][0]
        self.newlines = [line for line in templines if line[0] > self.seentime]
        self.seentime = oldesttime

    def getnewlines(self):
        return [(self.shortname, line[0], line[1]) for line in self.newlines]

    def formatline(self, line):
        dockerTimestamp = line[:26]
        convtime = datetime.datetime.strptime(dockerTimestamp, "%Y-%m-%dT%H:%M:%S.%f")
        return (convtime, line[31:])

def get_containers(names):
    return [ContainerInfo(name) for name in names]

def main():
    parser = OptionParser()
    parser.add_option("-c", "--containers", dest="containers",
                  help="write report to FILE", metavar="FILE")

    (options, args) = parser.parse_args()
    
    containerInfos = get_containers(args)

    while 1:
        lines = []
        for container in containerInfos:
            container.update()
            lines.extend(container.getnewlines())

        lines = sorted(lines, key=lambda x: x[1])
        for line in lines:
            print("{1:%y%m%d %H:%M:%S} {0}: {2}".format(line[0], line[1], line[2]))
        
        lines.clear()

        sleep (1)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        # Ctrl-C closes
        pass


