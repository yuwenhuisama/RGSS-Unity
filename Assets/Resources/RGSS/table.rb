require 'type_check_util'

class Table
  include TypeCheckUtil

  attr_reader :handler

  def initialize(xsize, ysize=0, zsize=0)
    check_arguments([xsize, ysize, zsize], [Integer, Integer, Integer])
    @handler = Unity::Table.new_xyz(xsize, ysize, zsize)
  end

  def resize(xsize, ysize=0, zsize=0)
    check_arguments([xsize, ysize, zsize], [Integer, Integer, Integer])
    @handler.resize(xsize, ysize, zsize)
  end

  def [](*args)
    if args.size == 1
      check_arguments(args, [Integer])
      x, = args
      @handler.get_x(x)
    elsif args.size == 2
      check_arguments(args, [Integer, Integer])
      x, y = args
      @handler.get_xy(x, y)
    elsif args.size == 3
      check_arguments(args, [Integer, Integer, Integer])
      x, y, z = args
      @handler.get_xyz(x, y, z)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  [:xsize, :ysize, :zsize].each do |prop|
    define_method(prop) { @handler.send(prop) }
  end

end