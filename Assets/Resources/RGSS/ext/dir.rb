class Dir

  def self.glob(pattern, flags=0, &block)
    if pattern.kind_of? Array
      patterns = pattern
    else
      return [] if pattern.empty?
      patterns = [pattern]
    end

    matches = []
    index = 0

    patterns.each do |pat|
      Dir::Glob.glob pat, flags, matches

      index += matches.size
    end

    if block
      matches.each(&block)
      return nil
    end

    return matches
  end

end
