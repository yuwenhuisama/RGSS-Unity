require 'type_check_util'

class Rect
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(*args)
    if args.size == 0
      @__handler__ = Unity::Rect.new_xywh(0, 0, 0, 0)
    elsif args.size == 1
      check_arguments(args, [Unity::Rect])
      r, = args
      @__handler__, = r
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      x, y, w, h = args
      @__handler__ = Unity::Rect.new_xywh(x, y, w, h)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def set(*args)
    if args.size == 1
      check_arguments(args, [Rect])
      rect, = args

      @__handler__.set_xywh(rect.x, rect.y, rect.width, rect.height)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])

      @__handler__.set_xywh(*args)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def empty
    @__handler__.set_xywh(0, 0, 0, 0)
  end

  def eql?(other)
    unless other.is_a?(Rect)
      return false
    end
    if self == other || self.__handler__ == other.__handler__
      return true
    end
    self.x == other.x && self.y == other.y && self.width == other.width && self.height == other.height
  end

  def hash
    @__handler__.hash
  end

  def _dump(d = 0)
    [self.x, self.y, self.width, self.height].pack('l4')
  end

  def self._load(s)
    Rect.new(*s.unpack('l4'))
  end

  [:x, :y, :width, :height].each do |prop|
    define_method(prop) do
      @__handler__.send(prop)
    end

    define_method("#{prop}=") do |value|
      check_type(value, Integer)
      @__handler__.send("#{prop}=", value)
    end
  end
end