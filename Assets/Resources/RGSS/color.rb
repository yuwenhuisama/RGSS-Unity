require 'type_check_util'

class Color
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(*args)
    if args.size == 0
      @__handler__ = Unity::Color.new_rgba(0, 0, 0, 0)
    elsif args.size == 1
      check_arguments(args, [Unity::Color])
      c, = args
      @__handler__, = c
    elsif args.size == 3
      check_arguments(args, [[Integer, Float], [Integer, Float], [Integer, Float]])
      r, g, b = args
      @__handler__ = Unity::Color.new_rgba(r, g, b, 255)
    elsif args.size == 4
      check_arguments(args, [[Integer, Float], [Integer, Float], [Integer, Float], [Integer, Float]])
      r, g, b, a = args
      @__handler__ = Unity::Color.new_rgba(r, g, b, a)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def set(*args)
    if args.size == 1
      check_arguments(args, [Color])
      color, = args

      @__handler__.set_rgba(color.red, color.green, color.blue, color.alpha)
    elsif args.size == 4
      check_arguments(args, [[Integer, Float], [Integer, Float], [Integer, Float], [Integer, Float]])

      r, g, b, a = args
      @__handler__.set_rgba(r, g, b, a)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def eql?(other)
    unless other.is_a? Color
      return false
    end

    if self == other || self.__handler__ == other.__handler__
      true
    end

    self.red == other.red && self.green == other.green && self.blue == other.blue && self.alpha == other.alpha
  end

  def hash
    @__handler__.hash
  end

  [:red, :blue, :green, :alpha].each do |prop|
    define_method("#{prop}=") do |value|
      check_type(value, [Integer, Float])
      v = value.clamp(0, 255)
      @__handler__.send(prop, v)
    end
    define_method(prop) do
      @__handler__.send(prop)
    end
  end

  def _dump(d = 0)
    [self.red, self.green, self.blue, self.alpha].pack('d4')
  end

  def self._load(s)
    Color.new(*s.unpack('d4'))
  end

  RED = Color.new(255, 0, 0, 255)
  GREEN = Color.new(0, 255, 0, 255)
  BLUE = Color.new(0, 0, 255, 255)
  WHITE = Color.new(255, 255, 255, 255)
  BLACK = Color.new(255, 255, 255, 255)
end
