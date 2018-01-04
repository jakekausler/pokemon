import json
from dateutil import parser
import datetime
import pytz
import math
import urllib2

url = "http://adventofcode.com/2017/leaderboard/private/view/105906.json"
SESSION = (
    "53616c7465645f5f73849f062b4c5c3268cd8e67b1"
    "24e01119fb5589598b6076bc58f8ab03292a345204b975c6fa639e"
)
inputfile = "json.json"
output = "json.csv"
days = 11


def downloadJson(url, session, inputfile):
    u = url
    request = urllib2.Request(
        u,
        headers={
            "cookie": (
                "session=" + session + ";"
                " _ga=GA1.2.1625535529.1512100335;"
                " _gid=GA1.2.527143979.1512264865; _gat=1"
            )
        }
    )
    contents = urllib2.urlopen(request).read()
    with open(inputfile, "w") as f:
        f.write(contents)


downloadJson(url, SESSION, inputfile)
data = json.load(open(inputfile))


class Person:
    def __init__(self, name, times):
        self.name = name
        times.sort()
        for t in range(len(times)):
            times[t][2] = parser.parse(times[t][2]).replace(tzinfo=pytz.UTC)
            startTime = datetime.datetime(2017, 12, times[t][0]).replace(tzinfo=pytz.UTC)
            times[t].append(times[t][2] - startTime)
        self.times = times
        self.dayStarMap = {}
        for time in self.times:
            self.dayStarMap[(time[0], time[1])] = (time[2], time[3])

    def __str__(self):
        s = self.name + "\n"
        for time in self.times:
            s += 'Day ' + str(time[0]).ljust(2) + ' Star ' + str(time[1]) + ': ' + str(time[3]) + "\n"
        return s[:-1]

    def getTimes(self, day):
        s = ""
        times = []
        for i in range(1, day + 1):
            for j in range(1, 3):
                if (i, j) in self.dayStarMap:
                    totalSeconds = int(math.floor(self.dayStarMap[(i, j)][1].total_seconds()))
                    days = totalSeconds/86400
                    totalSeconds -= days*86400
                    hours = totalSeconds/3600
                    totalSeconds -= hours*3600
                    minutes = totalSeconds/60
                    totalSeconds -= minutes*60
                    seconds = totalSeconds
                    hours = hours + days * 24
                    s += str(str(hours).zfill(3) + ":" + str(minutes).zfill(2) + ":" + str(seconds).zfill(2)) + "\t"
                    if j == 2:
                        times.append(self.dayStarMap[(i, j)][1])
                else:
                    s += "\t"
                    if j == 2:
                        times.append(datetime.datetime.now().replace(tzinfo=pytz.UTC) - datetime.datetime(2017, 12, i).replace(tzinfo=pytz.UTC))
        times.sort()
        totalSeconds = 0
        if len(times) % 2 == 0:
        	totalSeconds = int(math.floor(((times[len(times)/2] + times[len(times)/2 - 1]) / 2).total_seconds()))
        else:
        	totalSeconds = int(math.floor(times[len(times)/2].total_seconds()))
        days = totalSeconds/86400
        totalSeconds -= days*86400
        hours = totalSeconds/3600
        totalSeconds -= hours*3600
        minutes = totalSeconds/60
        totalSeconds -= minutes*60
        seconds = totalSeconds
        hours = hours + days * 24
        s += str(str(hours).zfill(3) + ":" + str(minutes).zfill(2) + ":" + str(seconds).zfill(2))
        return s

people = []

for member in data["members"]:
    name = data["members"][member]["name"]
    times = []
    for day in data["members"][member]["completion_day_level"]:
        for star in data["members"][member]["completion_day_level"][day]:
            time = data["members"][member]["completion_day_level"][day][star]["get_star_ts"]
            times.append([int(day), int(star), time])
    people.append(Person(name, times))


s = "\t"
for i in range(days):
    s += (str(i+1) + ".1") + "\t"
    s += (str(i+1) + ".2") + "\t"
s = s + "Median\n"
for person in people:
    s += person.name + "\t" + person.getTimes(days) + "\n"
with open(output, "w") as f:
    f.write(s)
