require 'type_check_util'

class Color
  include TypeCheckUtil

  attr_reader :handler

  def initialize(*args)
    if args.size == 1
      check_arguments(args, [Unity::Color])
      h, = args
      @handler, = Unit::Color.new_rgba(h.r, h.g, h.b, h.a)
    elsif args.size == 3
      check_arguments(args, [Integer, Integer, Integer])
      r, g, b = args
      @handler = Unity::Color.new_rgba(r, g, b, 255)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      r, g, b, a = args
      @handler = Unity::Color.new_rgba(r, g, b, a)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def set(*args)
    if args.size == 1
      check_arguments(args, [Color])
      color, = args

      @handler.set_rgba(color.red, color.green, color.blue, color.alpha)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])

      r, g, b, a = args
      @handler.set_rgba(r, g, b, a)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def eql?(other)
    unless other.is_a? Color
      return false
    end

    if self == other || self.handler == other.handler
      true
    end

    self.red == other.red && self.green == other.green && self.blue == other.blue && self.alpha == other.alpha

  end

  def hash
    @handler.hash
  end

  [:red, :blue, :green, :alpha].each do |prop|
    define_method("#{prop}=") do |value|
      check_type(value, Integer)
      v = value.clamp(0, 255)
      @handler.send(prop, v)
    end
    define_method(prop) do
      @handler.send(prop)
    end
  end

  RED = Color.new(255, 0, 0, 255)
  GREEN = Color.new(0, 255, 0, 255)
  BLUE = Color.new(0, 0, 255, 255)
  WHITE = Color.new(255, 255, 255, 255)
  BLACK = Color.new(255, 255, 255, 255)
end
