import sys
import re

def remove_class_from_file(file_path, class_name):
    with open(file_path, 'r') as file:
        lines = file.readlines()

    class_pattern = re.compile(r'\bclass\s+' + re.escape(class_name) + r'\b')
    start_index = -1
    brace_count = 0

    for i, line in enumerate(lines):
        if start_index == -1 and class_pattern.search(line):
            start_index = i
            brace_count = line.count('{') - line.count('}')
        elif start_index != -1:
            brace_count += line.count('{') - line.count('}')
            if brace_count == 0:
                end_index = i
                break
    else:
        print(f"Class '{class_name}' not found in the file.")
        return
    
    # find attributes in lines between the start index and 10 lines preceeding
    # stop search if we come across an end brace or semicolon
    end_search = max(0, start_index - 30)
    start_search = min(len(lines), start_index)
    for i in range(start_search, end_search, -1):
        contains_semicolon_or_brace = re.search(r'[;{}]', lines[i])
        if contains_semicolon_or_brace:
            break
        start_index = i 


    print(f"Class '{class_name}' has been removed from the file. Removed:\n")
    for line in lines[start_index:end_index + 1]:
        print(line, end='')
    del lines[start_index:end_index + 1]

    with open(file_path, 'w') as file:
        file.writelines(lines)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python remove_class.py <file_path> <class_name>")
        sys.exit(1)

    file_path = sys.argv[1]
    class_name = sys.argv[2]
    remove_class_from_file(file_path, class_name)