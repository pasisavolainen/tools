#!/usr/bin/env python3

from optparse import OptionParser
import subprocess
import datetime
from time import sleep
import re

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
        # "[2019-05-23 07:03:31 INF]"
        self.tstamp_rx = re.compile("^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} .{3}\]")

    def update(self):
        #print ("*** Updating {s.name}:{s.shortname}".format(s=self))

        proc = subprocess.run(["docker", "logs", "-t", "--tail", "50", self.name],
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
        # if container doesn't exist, the returned string is something like 'Error: no such container'
        if not dockerTimestamp[0].isdigit():
            return (self.seentime, line)
        convtime = datetime.datetime.strptime(dockerTimestamp, "%Y-%m-%dT%H:%M:%S.%f")
        stamplessline = line[31:]
        if (self.tstamp_rx.match(stamplessline)):
            msglvl = stamplessline[21:24]
            stamplessline = "{0} {1}".format(msglvl, stamplessline[26:])
        return (convtime, stamplessline)

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
            service, timestamp, msg = line
            print("{1: %H:%M:%S}.{3:02d} <{0}> {2}".format(service, timestamp, msg, int(timestamp.microsecond/10000)))

        lines.clear()

        sleep (1)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        # Ctrl-C closes
        pass


