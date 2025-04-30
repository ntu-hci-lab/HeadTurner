##################################################################################################
# Plan 4 (Using)
# Instead of analyzing the head rotation position and rotation angle of each turn, 
#  which was the goal of our previous plan, we will focus on analyzing the position 
#  where the head remains stationary in this plan. This will be presented by drawing 
## stacked bar charts showing different directions.

# Using and Reading Method of the Following Code:

# 1. Using
# Step 1: To use the code to draw stacked bar charts, you need to copy the code from 
#   the handle_outliers function to the Stacked_Bar_Plotter function, which spans from 
#   line 51 to 369 (inclusive), and paste it into the Console. Ensure that you have 
#   RStudio installed; if not, please refer to the readme.txt file. These are the 
#   functions needed to analyze the data and generate the graph.
# Step 2: Next, copy and paste the code in line 261 into the Console. This will invoke 
#   the categorization code. A prompt will appear in the Console asking you to enter the number 
#   of apps you wish to analyze. After entering this number, open the .csv files one by one,
#   make sure that the name of your files is the corresponding name of the apps.
# Step 3: Once all the files have been opened and the code execution is complete, copy and 
#   paste the code from line 374 into the Console. This code will invoke the plotting function. 
#   Type one of the three directions (Pitch, Yaw, Roll) to generate the final plot.
# Step 4: To obtain plots for all three directions, repeat the process by copying and pasting 
#   the code in line 377 after completing Steps 1 and 2. Enter "Pitch," "Yaw," and "Roll" 
#   each time to generate the respective plots.

# 2. Reading
# The following code performs two main tasks: categorizing the rotation angles and presenting 
#   them through stacked bar charts. It is divided into three main parts:
# PART 1 is responsible for categorization:
#   PART 1.1: This is the primary code for recording and analyzing data. It processes raw pitch, 
#     yaw, and roll data, categorizing them into different directional ranges. There are six 
#     ranges in degrees:
#     0 - 5 (inclusive)
#     5 - 10 (excluding 5, including 10)
#     10 - 15 (excluding 10, including 15)
#     15 - 20 (excluding 15, including 20)
#     20 - 25 (excluding 20, including 25)
#     > 25 (greater than 25)
#   PART 1.2: This code requires the user to open the .csv files and uses the functions from 
#     PART 1.1 to perform the categorization.
# PART 2: This is the plotting code. You should have ggplot2 installed to successfully generate 
#     the plots.

##################################################################################################
## PART 1.1: CATEGORIZATION FUNCTIONS
##################################################################################################

## handle_outliers deals with the data that's greater than 180 or smaller than -180,
##  and returns the list contains pitch_vec, yaw_vec and roll_vec in this order.
handle_outliers <- function(pitch_vec, yaw_vec, roll_vec, valid_row, valid_length) {
  
  for (i in valid_row:valid_length) {
    ## pitch
    if (pitch_vec[i] > 180) {
      pitch_vec[i] <- pitch_vec[i] - 360
    } 
    else if (pitch_vec[i] < -180) {
      pitch_vec[i] <- pitch_vec[i] + 360
    }
    ## yaw
    if (yaw_vec[i] > 180) {
      yaw_vec[i] <- yaw_vec[i] - 360
    } 
    else if (yaw_vec[i] < -180) {
      yaw_vec[i] <- yaw_vec[i] + 360
    }
    ## roll
    if (roll_vec[i] > 180) {
      roll_vec[i] <- roll_vec[i] - 360
    } 
    else if (roll_vec[i] < -180) {
      roll_vec[i] <- roll_vec[i] + 360
    }
  } 
  data_list <- list(pitch_vec, yaw_vec, roll_vec)
  return(data_list)
}

## determine_range is a helper function of position_counter, it categorizes 
##  the angles in different ranges, as the introduction in the beginning, 
##  we have a total of six different ranges.
determine_range <- function(position_vec, counting_list) {
  for (i in 1:length(position_vec)) {
    if (0 <= abs(position_vec[i]) && abs(position_vec[i]) <= 5) {
      counting_list[[1]] <- counting_list[[1]] + 1
    } else if (5 < abs(position_vec[i]) && abs(position_vec[i]) <= 10) {
      counting_list[[2]] <- counting_list[[2]] + 1
    } else if (10 < abs(position_vec[i]) && abs(position_vec[i]) <= 15) {
      counting_list[[3]] <- counting_list[[3]] + 1
    } else if (15 < abs(position_vec[i]) && abs(position_vec[i]) <= 20) {
      counting_list[[4]] <- counting_list[[4]] + 1
    } else if (20 < abs(position_vec[i]) && abs(position_vec[i]) <= 25) {
      counting_list[[5]] <- counting_list[[5]] + 1
    } else if (abs(position_vec[i]) > 25) {
      counting_list[[6]] <- counting_list[[6]] + 1
    } else {
      stop("determine_range, angle range > 180 degree.")
    }
  }
  return(counting_list)
}

## This function categorizes the positions pitch, yaw and roll to one
##  of the six ranges in degrees and put all the results into a list.
position_counter <- function(position_list, valid_row, valid_length) {
  ## handle_outliers <- function(pitch_vec, yaw_vec, roll_vec, valid_row, valid_length)
  ## Index : Range
  ## 1: 0 - 5
  ## 2: 5 - 10
  ## 3: 10 - 15
  ## 4: 15 - 20
  ## 5: 20 - 25
  ## 6: > 25
  position_list <- handle_outliers(position_list[[1]], position_list[[2]], position_list[[3]],
                                   valid_row, valid_length)
  pitch_count <- list(0,0,0,0,0,0)
  yaw_count <- list(0,0,0,0,0,0)
  roll_count <- list(0,0,0,0,0,0)
  
  for (i in 1:length(position_list)) {
    
    if (i == 1) {
      pitch_count <- determine_range(position_list[[i]], pitch_count)
    }
    else if (i == 2) {
      yaw_count <- determine_range(position_list[[i]], yaw_count)
    }
    else if (i == 3) {
      roll_count <- determine_range(position_list[[i]], roll_count)
    }
    else {
      stop("position_counter, length out of range 3.")
    }
  }
  pack_list <- list(pitch_count, yaw_count, roll_count)
  return(pack_list)
}



##################################################################################################
## PART 1.2: CATEGORIZATION IMPLEMENTATION
##################################################################################################

## Deals with one direction of many users' total when using one same app.
Adder <- function(user_list, dir_idx, users_num) {
  sum_list <- list(0,0,0,0,0,0)
  for (i in 1:users_num) {
    for (j in 1:6) {
      sum_list[[j]] <- sum_list[[j]] + user_list[[i]][[dir_idx]][[j]]
    }
  }
  return(sum_list)
}

## Deals with one direction of many users' average when using one same app.
Average_Calculator <- function(sum_list, user_num) {
  for (i in 1:6) {
    sum_list[[i]] <- sum_list[[i]] / user_num
  }
  return(sum_list)
}

## Combines three directions of many users' average when using one same app into one list.
Average_Counter <- function(user_list) {
  user_num <- length(user_list)
  average_list <- list()
  pitch_total <- Adder(user_list, 1, user_num)
  yaw_total <- Adder(user_list, 2, user_num)
  roll_total <- Adder(user_list, 3, user_num)    
  
  pitch_average <- Average_Calculator(pitch_total, user_num)
  yaw_average <- Average_Calculator(yaw_total, user_num)
  roll_average <- Average_Calculator(roll_total, user_num)
  
  return(list(pitch_average, yaw_average, roll_average))
  
}

Analysis_Average_Func <- function() {
  
  trying_times <- 0
  trying_limit <- 3 
  while (TRUE) {
    input_val <- readline(prompt = "Please enter the number of users that need to be analyzed (Integer > 0 Only): ")
    
    num_val <- as.numeric(input_val)
    
    if (!is.na(num_val) && num_val > 0 && num_val == as.integer(num_val)) {
      break
    }
    
    trying_times <- trying_times + 1
    if (trying_times == trying_limit) {
      stop("Invalid Number. Too many tries.")
    }
  } 
  user_num <- as.numeric(input_val)
  
  app_name <- ""
  while (TRUE) {
    app_name <- readline(prompt = "Please enter the app's name: ")
    change_input <- readline(prompt = "Do you want to change the app's name? Enter 'no' if you don't: ")
    if (tolower(change_input) == "no") {
      break
    }
  }
  
  status_list <- list()
  
  for (j in 1:2) {
    user_list <- list()
    
    if (j == 1) {
      print("Choose files when Head Turner is on.")
    }
    else {
      print("Choose files when Head Turner is off.")
    }
    
    for (i in 1:user_num) {
      print(paste("Current: ", i))
      f <- file.choose()
      app_data <- read.csv(f)
      
      pitch_data <- app_data$HeadPitch
      yaw_data <- app_data$HeadYaw
      roll_data <- app_data$HeadRoll
      app_time <- app_data$time
      
      valid_data_length <- length(app_time)
      
      app_pack_list <- position_counter(list(pitch_data, yaw_data, roll_data), 1, valid_data_length)
      user_list[[i]] <- app_pack_list
    }
    
    one_status_count <- Average_Counter(user_list)
    status_list[[j]] <- one_status_count
  }
  
  status_list[[3]] <- app_name
  return(status_list)
}




##################################################################################################
## PART 2: PLOTTING
##################################################################################################

## Self Defined Range
range_list <- list(">25", "20-25", "15-20", "10-15", "5-10", "0-5")

Stacked_Bar_Plotter <- function(status_list) {
  app_name <- status_list[[3]][[1]]
  status <- ""
  library(ggplot2)
  # Create an empty data frame to store the counts
  data <- data.frame(
    App = character(),
    Range = character(),
    Proportion = numeric(),
    stringsAsFactors = FALSE
  )
  
  
  trying_limit <- 3
  trying_time <- 0
  
  while (TRUE) {
    direction <- readline(prompt = "Please enter the direction in one word (One of pitch, yaw or roll) eg. pitch: ")
    if (tolower(direction) == "pitch") {
      dir_idx <- 1
      break
    }
    else if (tolower(direction) == "yaw") {
      dir_idx <- 2
      break
    }
    else if (tolower(direction) == "roll") {
      dir_idx <- 3
      break
    }
    
    trying_time <- trying_time + 1
    
    if (trying_time == trying_limit) {
      stop("Wrong direction name. Too many tries.")
    } else {
      print("Wrong direction name, please enter one of pitch, yaw or roll.")
    }
  }
  trying_limit <- 3
  trying_time <- 0
  while (TRUE) {
    part <- readline(prompt = "Is this head movement or body movement? (Enter either 'head' or 'body'): ")
    if (tolower(part) == "head") {
      part <- "Head"
      break
    }
    else if (tolower(part) == "body") {
      part <- "Body"
      break
    }
    trying_time <- trying_time + 1
    
    if (trying_time == trying_limit) {
      stop("Wrong part name. Too many tries.")
    } else {
      print("Wrong part name, please enter either 'head' or 'body'")
    }
    
  }
  # Iterate through each app, calculating the proportion of each action
  for (i in 1:2) {
    if (i == 1) {
      status <- "On"
    }
    else {
      status <- "Off"
    }
    app_data <- status_list[[i]][[dir_idx]] 
    
    total_angles <- sum(unlist(app_data))
    
    if (total_angles == 0) {
      total_angles <- 1 
    }
    # Add proportion data to the data frame
    data <- rbind(data, data.frame(App = status, Range = range_list[[6]], Proportion = app_data[[1]] / total_angles))
    data <- rbind(data, data.frame(App = status, Range = range_list[[5]], Proportion = app_data[[2]] / total_angles))
    data <- rbind(data, data.frame(App = status, Range = range_list[[4]], Proportion = app_data[[3]] / total_angles))
    data <- rbind(data, data.frame(App = status, Range = range_list[[3]], Proportion = app_data[[4]] / total_angles))
    data <- rbind(data, data.frame(App = status, Range = range_list[[2]], Proportion = app_data[[5]] / total_angles))
    data <- rbind(data, data.frame(App = status, Range = range_list[[1]], Proportion = app_data[[6]] / total_angles))
    
    print(paste(status, "0-5: ", app_data[[1]] / total_angles))
    print(paste(status, "5-10: ", app_data[[2]] / total_angles))
    print(paste(status, "10-15: ", app_data[[3]] / total_angles))
    print(paste(status, "15-20: ", app_data[[4]] / total_angles))
    print(paste(status, "20-25: ", app_data[[5]] / total_angles))
    print(paste(status, ">25: ", app_data[[6]] / total_angles))
  }
  
  data$Range <- factor(data$Range, levels = c(range_list[[1]], range_list[[2]], range_list[[3]], 
                                              range_list[[4]], range_list[[5]], range_list[[6]]))
  
  ggplot(data, aes(x = App, y = Proportion, fill = Range)) +
    geom_bar(stat = "identity") +
    scale_fill_manual(name = "Range in Degrees", 
                      values = c(">25" = "darkblue", 
                                 "20-25" = "mediumblue", 
                                 "15-20" = "blue", 
                                 "10-15" = "dodgerblue", 
                                 "5-10" = "deepskyblue", 
                                 "0-5" = "lightskyblue")) + 
    labs(title = paste("Proportion of",
                       part,
                       "Movement in Different Angle Ranges \nin the", 
                       direction, 
                       "Direction When Using App named ",
                       app_name,
                       "Comparing Head Turner is On or Off"),
         x = "App",
         y = "Proportion") +
    theme_minimal()
}

##################################################################################################
## Envoking Functions Above
## 1 Time
status_list <- Analysis_Average_Func()

## 3 Times(1 time for each direction, Pitch, Yaw or Roll)
Stacked_Bar_Plotter(status_list)
