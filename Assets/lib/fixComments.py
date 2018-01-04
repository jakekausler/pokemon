FILE = "scripts/battle/battlemove.cs"

lines = []

with open(FILE) as f:
    lines = f.readlines()
    for i in range(len(lines)):
        if lines[i][:2] == "/*" and lines[i + 1][-2] != "*":
            comments = []
            j = i + 1
            while lines[j][:2] != "*/":
                comments.append([lines[j].replace("*", "")[:-1], j])
                j += 1
            maxLength = max([len(c[0]) for c in comments])
            print maxLength
            start = "/*" + "*" * maxLength + "**"
            print start
            for k in range(len(comments)):
                comments[k][0] = "* " + comments[k][0] + " " * (maxLength-len(comments[k][0])) + " *"
                print comments[k][0]
            end = "**" + "*" * maxLength + "*/"
            print end
            lines[i] = start + "\n"
            for comment in comments:
                lines[comment[1]] = comment[0] + "\n"
            lines[comments[len(comments) - 1][1] + 1] = end + "\n"
        # print lines[i][:-1]

with open(FILE, "w") as f:
    f.write("".join(lines))