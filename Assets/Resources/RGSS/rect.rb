require 'type_check_util'

class Rect
  include TypeCheckUtil

  attr_reader :handler

  def initialize(*args)
    if args.size == 1
      check_arguments(args, [Unity::Rect])
      h, = args
      @handler, = Unity::Rect.new_xywh(h.x, h.y, h.w, h.h)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      x, y, w, h = args
      @handler = Unity::Rect.new_xywh(x, y, w, h)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def set(*args)
    if args.size == 1
      check_arguments(args, [Rect])
      rect, = args

      self.set_xywh(rect.x, rect.y, rect.w, rect.h)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])

      self.x, self.y, self.w, self.h = args
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def empty
    self.x, self.y, self.w, self.h = 0, 0, 0, 0
  end

  def eql?(other)
    if self == other
      true
    end
    self.handler == other.handler
  end

  def hash
    @handler.hash
  end

  [:x, :y, :w, :h].each do |prop|
    define_method(prop) do
      @handler.send(prop)
    end

    define_method("#{prop}=") do |value|
      check_type(value, Integer)
      @handler.send("#{prop}=", value)
    end
  end
end