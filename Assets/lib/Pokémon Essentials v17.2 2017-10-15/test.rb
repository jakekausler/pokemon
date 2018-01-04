File.open('data/types.dat', "rb") { |f|
  obj = Marshal.load(f)
  print obj[2].length
}